using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Constants;
using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Helpers;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace GoldCasino.ApiModule.Auth;

public class EncryptedCookieHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	CookieEncryptionHelper crypto,
	IBusinessApiService businessApi,
	AuthService authService
		, IAuthCookieService authCookie
	) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
	private readonly CookieEncryptionHelper _crypto = crypto;
	private readonly IBusinessApiService _businessApi = businessApi;
	private readonly AuthService _authService = authService;
	private readonly IAuthCookieService _authCookie = authCookie;
	private const string LidRedirectItemKey = LidAuthenticationContext.RedirectUrlItemKey;

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var lid = ExtractLidFromQuery();
		Credential? credential = null;

		if (Request.Cookies.TryGetValue(AuthConstants.CookieName, out var token))
		{
			if (!_crypto.TryDecrypt<Credential>(token, out credential))
			{
				if (lid is null)
					return AuthenticateResult.Fail("Invalid credential cookie");
				Logger.LogWarning("Failed to decrypt credential cookie, falling back to lid authentication.");
			}
		}
		else if (lid is null)
		{
			return AuthenticateResult.NoResult();
		}

		if (lid is not null)
		{
			var lidResult = await TryAuthenticateWithLidAsync(lid, credential);
			if (lidResult is { } result)
				return result;
		}

		if (credential is null)
			return AuthenticateResult.NoResult();

		return AuthenticateResult.Success(BuildTicket(credential));
	}

	private async Task<AuthenticateResult?> TryAuthenticateWithLidAsync(string lid, Credential? credential)
	{
		try
		{
			var decryptResult = await _businessApi.GeneralDecrypt(lid);
			if (!decryptResult.IsSuccess || decryptResult.Value is null)
			{
				Logger.LogWarning("Failed to decrypt lid parameter");
				return null;
			}

			var decrypted = decryptResult.Value;

			if (credential is not null && CredentialsMatch(credential, decrypted))
			{
				var updated = credential with { Lid = lid };
				ScheduleLidRemovalRedirect();
				return AuthenticateResult.Success(BuildTicket(updated));
			}

			var loginResult = await _authService.SignInAsync(lid);
			if (!loginResult.IsSuccess || loginResult.Value is null)
			{
				Logger.LogWarning("Sign in with lid failed: {Error}", loginResult.Error?.Message ?? "Unknown error");
				return null;
			}

			var login = loginResult.Value;
			
			// Fetch affiliate ID for the logged-in user
			var affiliateId = await FetchAffiliateIdAsync(login.EntityId);

			var freshCredential = new Credential(login.EntityId, login.Username, login.Password, login.Lid, affiliateId);
			_authCookie.Set(freshCredential);
			ScheduleLidRemovalRedirect();
			return AuthenticateResult.Success(BuildTicket(freshCredential));
		}
		catch (UpstreamServiceException ex)
		{
			Logger.LogError(ex, "Upstream error during lid authentication");
			return null;
		}
		catch (Exception ex)
		{
			Logger.LogError(ex, "Unexpected error during lid authentication");
			return null;
		}
	}

	private async Task<int> FetchAffiliateIdAsync(string entityId)
	{
		try
		{
			var affiliateResult = await _businessApi.EntityFindAsync(new()
			{
				Fields = FieldHelper<UserAffiliateDto>.Fields,
				Filter = new() { { "entityId", entityId } }
			});

			if (affiliateResult.IsSuccess && affiliateResult.Value?.Entities.Count > 0)
			{
				return affiliateResult.Value.Entities[0].AffiliateID;
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning(ex, "Failed to fetch affiliate ID for user {EntityId}", entityId);
		}
		return 0;
	}

	private string? ExtractLidFromQuery()
	{
		if (!Request.Query.TryGetValue("lid", out var lidValues))
			return null;

		var lid = lidValues.ToString();
		return string.IsNullOrWhiteSpace(lid) ? null : lid;
	}

	private static bool CredentialsMatch(Credential credential, GeneralDecrypt decrypted)
	{
		var entityId = decrypted.EntityId.ToString(CultureInfo.InvariantCulture);
		return string.Equals(credential.EntityId, entityId, StringComparison.Ordinal)
			&& string.Equals(credential.Username, decrypted.Username, StringComparison.OrdinalIgnoreCase)
			&& string.Equals(credential.Password, decrypted.Password, StringComparison.Ordinal);
	}

	private void ScheduleLidRemovalRedirect()
	{
		if (!HttpMethods.IsGet(Request.Method) && !HttpMethods.IsHead(Request.Method))
			return;
		if (Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
			return;
		var targetUrl = BuildUrlWithoutLid();
		if (targetUrl is null)
			return;
		Context.Items[LidRedirectItemKey] = targetUrl;
	}

	private string? BuildUrlWithoutLid()
	{
		if (!Request.QueryString.HasValue)
			return null;
		var parsed = QueryHelpers.ParseQuery(Request.QueryString.Value);
		var hasLid = parsed.Keys.Any(k => string.Equals(k, "lid", StringComparison.OrdinalIgnoreCase));
		if (!hasLid)
			return null;
		var newQuery = QueryString.Empty;
		foreach (var (key, values) in parsed)
		{
			if (string.Equals(key, "lid", StringComparison.OrdinalIgnoreCase))
				continue;
			foreach (var value in values)
			{
				newQuery = newQuery.Add(key, value ?? string.Empty);
			}
		}
		return string.Concat(Request.Path, newQuery);
	}

	private AuthenticationTicket BuildTicket(Credential cred)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, cred.EntityId),
			new(ClaimTypes.Name, cred.Username),
			new("Lid", cred.Lid),
			new("Password", cred.Password),
			new("AffiliateId", cred.AffiliateId.ToString())
		};

		var identity = new ClaimsIdentity(claims, Scheme.Name);
		var principal = new ClaimsPrincipal(identity);
		return new AuthenticationTicket(principal, Scheme.Name);
	}
}

record LoginDto(string EntityId, string Username, string Password);
public record Credential(string EntityId, string Username, string Password, string Lid, int AffiliateId = 0);

internal static class LidAuthenticationContext
{
	public const string RedirectUrlItemKey = "__lidRedirectUrl";
}