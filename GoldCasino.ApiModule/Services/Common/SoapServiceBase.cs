using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Common.Patching;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.BusinessApi.Policies;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog;

namespace GoldCasino.ApiModule.Services.Common;

internal abstract class SoapServiceBase
{
	private const int PayloadLogLimit = 4000;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private static string TrimPayload(string payload)
	{
		if (string.IsNullOrEmpty(payload)) return "<empty>";
		return payload.Length > PayloadLogLimit ? payload[..PayloadLogLimit] : payload;
	}

	/// Escapes unescaped control characters inside JSON string literals.
	private static readonly Regex ControlCharRegex = new(@"[\x00-\x1f]", RegexOptions.Compiled);

	private static string SanitizeJsonControlChars(string json) =>
		!ControlCharRegex.IsMatch(json) ? json
		: Regex.Replace(json, @"""(?:[^""\\]|\\.)*""", m =>
			ControlCharRegex.Replace(m.Value, c => $"\\u{(int)c.Value[0]:x4}"));

	protected static Result<TSuccess, Error> ParseJson<TSuccess>(string json)
	{
		json = json.TrimStart();
		json = SanitizeJsonControlChars(json);
		JsonElement root;
		try
		{
			using var doc = JsonDocument.Parse(json);
			root = doc.RootElement.Clone(); // clone so we can dispose doc
		}
		catch (JsonException ex)
		{
			Log.Error(ex, "Invalid JSON when parsing SOAP response. Payload: {Payload}", TrimPayload(json));
			throw new UpstreamServiceException(ErrorCodes.ParseError, $"Invalid JSON: {ex.Message}", ex);
		}

		switch (root.ValueKind)
		{
			case JsonValueKind.Array:
				{
					try
					{
						var value = JsonSerializer.Deserialize<TSuccess>(root.GetRawText(), JsonOptions)!;
						return Result<TSuccess, Error>.Ok(value);
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to deserialize SOAP array payload. Payload: {Payload}", TrimPayload(json));
						return Result<TSuccess, Error>.Fail(new(ErrorCodes.UpstreamError, ex.Message));
					}
				}
			// case JsonValueKind.Object:
			// 	{

			// 		try
			// 		{
			// 			var obj = JsonSerializer.Deserialize<TSuccess>(root.GetRawText(), JsonOptions)!;

			// 			if (obj is ApiResponse resp && !resp.IsOk)
			// 			{
			// 				if (resp.ResultCode == -2) // Database Error
			// 					throw new UpstreamServiceException(
			// 							ErrorCodes.DatabaseError,
			// 							resp.ResultMessage ?? "Database error occurred",
			// 							resp.ResultCode,
			// 							obj);

			// 				if (resp.ResultCode == -1)
			// 					throw new AuthenticationServiceException(
			// 							resp.ResultMessage ?? "Authentication failed",
			// 							resp.ResultCode,
			// 							obj);

			// 				return Result<TSuccess, Error>.Fail(new SoapApiError(
			// 						ErrorCodes.UpstreamError,
			// 						resp.ResultMessage ?? "Unknown error",
			// 						resp.ResultCode,
			// 						obj));
			// 			}
			// 			return Result<TSuccess, Error>.Ok(obj);
			// 		}
					// catch (AuthenticationServiceException)
					// {
					// 	throw;
					// }
			// 		catch (Exception ex)
			// 		{
			// 			Console.WriteLine(ex);
			// 			return Result<TSuccess, Error>.Fail(new(ErrorCodes.UpstreamError, ex.Message));
			// 		}
			// 	}
			case JsonValueKind.Object:
				{
					try
					{
						// FIX: Check if the JSON Object is actually an API Error (ApiResponse)
						// regardless of what TSuccess is expected to be.
						if (root.TryGetProperty("ResultCode", out var resultCodeElement) &&
							resultCodeElement.ValueKind == JsonValueKind.Number)
						{
							var resultCode = resultCodeElement.GetInt32();

							// If ResultCode indicates failure (e.g., not 0), handle it as an error
							if (resultCode < 0)
							{
								// Deserialize specifically as ApiResponse to get the error message
								var errorResp = JsonSerializer.Deserialize<ApiResponse>(root.GetRawText(), JsonOptions)!;
								Log.Error("SOAP response returned failure code {ResultCode}. Payload: {Payload}", errorResp.ResultCode, TrimPayload(json));

								if (resultCode == -2) // Database Error
									throw new UpstreamServiceException(
													ErrorCodes.DatabaseError,
													errorResp.ResultMessage ?? "Database error occurred",
													errorResp.ResultCode,
													errorResp);

								if (resultCode == -1) // Auth Error
									throw new AuthenticationServiceException(
													errorResp.ResultMessage ?? "Authentication failed",
													errorResp.ResultCode,
													errorResp);

								return Result<TSuccess, Error>.Fail(new SoapApiError(
												ErrorCodes.UpstreamError,
												errorResp.ResultMessage ?? "Unknown error",
												errorResp.ResultCode,
												errorResp));
							}
						}

						// If we get here, it's not an error object (or ResultCode is 0).
						// NOW it is safe to attempt deserialization into TSuccess.
						var obj = JsonSerializer.Deserialize<TSuccess>(root.GetRawText(), JsonOptions)!;

						// Optional: If TSuccess IS ApiResponse, check IsOk one last time 
						// (in case you reuse this method for non-array types).
						if (obj is ApiResponse resp && !resp.IsOk)
						{
							// Handle specific edge case where TSuccess is compatible with ApiResponse
							// but we missed the check above (unlikely if logic matches).
							return Result<TSuccess, Error>.Fail(new(ErrorCodes.UpstreamError, resp.ResultMessage));
						}

						return Result<TSuccess, Error>.Ok(obj);
					}
					catch (AuthenticationServiceException)
					{
						throw;
					}
					catch (Exception ex)
					{
						Log.Error(ex, "Failed to deserialize SOAP object payload. Payload: {Payload}", TrimPayload(json));
						return Result<TSuccess, Error>.Fail(new(ErrorCodes.UpstreamError, ex.Message));
					}
				}
			default:
				Log.Error("Unexpected JSON kind {ValueKind} while parsing SOAP response. Payload: {Payload}", root.ValueKind, TrimPayload(json));
				throw new UpstreamServiceException(
						ErrorCodes.ParseError,
						$"Unexpected JSON kind: {root.ValueKind}");
		}
	}

	protected static async Task<Result<TSuccess, Error>> ExecuteAsync<TSuccess>(
			Func<Task<string>> soapCall)
	{
		string json;
		try
		{
			json = await soapCall();
		}
		catch (Exception ex)
		{
			Log.Error(ex, "SOAP client invocation failed before response was received");
			throw new UpstreamServiceException(
					ErrorCodes.UpstreamError,
					$"Upstream error: {ex.Message}",
					ex);
		}


		if (string.IsNullOrWhiteSpace(json))
		{
			Log.Error("SOAP call returned empty payload");
			throw new UpstreamServiceException(
					ErrorCodes.EmptyResponse,
					"Empty response from service");
		}

		var result = ParseJson<TSuccess>(json);
		if (!result.IsSuccess)
			Log.Error("SOAP response parsing returned error. Payload: {Payload}", TrimPayload(json));

		return result;
	}

	protected static async Task<string> ExecuteRawAsync(Func<Task<string>> soapCall)
	{
		string payload;
		try
		{ payload = await soapCall(); }
		catch (Exception ex)
		{
			Log.Error(ex, "SOAP client invocation failed before raw payload was received");
			throw new UpstreamServiceException(
					ErrorCodes.UpstreamError,
					$"Upstream error: {ex.Message}",
					ex);
		}


		if (string.IsNullOrWhiteSpace(payload))
		{
			Log.Error("SOAP call returned empty raw payload");
			throw new UpstreamServiceException(
					ErrorCodes.EmptyResponse,
					"Empty response from service");
		}

		return payload;
	}

	protected static (string[] Names, string[] Values) FlattenDtoToSoapArrays<TDto>(
			TDto dto, SoapUpdatePolicyBase policy)
	{
		var props = typeof(TDto).GetProperties(BindingFlags.Instance | BindingFlags.Public);
		var names = new List<string>(props.Length);
		var values = new List<string>(props.Length);

		foreach (var p in props)
		{
			if (!p.CanRead) continue;

			var jp = p.GetCustomAttribute<EntityFieldAttribute>();
			if (jp is null || string.IsNullOrWhiteSpace(jp.FieldName)) continue;

			var column = jp.FieldName;
			var rawVal = p.GetValue(dto);

			var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
			var isStringProp = t == typeof(string);

			// nulls
			if (rawVal is null)
			{
				if (isStringProp)
				{
					if (policy.ClearNullStrings && !policy.SkipEmptyStringColumns.Contains(column))
					{
						names.Add(column);
						values.Add(string.Empty);
					}
				}
				else if (policy.NonStringNulls == NonStringNullHandling.AsToken &&
						 !string.IsNullOrEmpty(policy.NullToken))
				{
					names.Add(column);
					values.Add(policy.NullToken!); // backend should treat as SQL NULL
				}
				continue;
			}

			// strings
			if (rawVal is string s)
			{
				if (string.IsNullOrWhiteSpace(s) && policy.SkipEmptyStringColumns.Contains(column))
					continue;

				names.Add(column);
				values.Add(s);
				continue;
			}

			// everything else
			names.Add(column);
			values.Add(PatchFormatter.Soap(rawVal));
		}

		return (names.ToArray(), values.ToArray());
	}

}