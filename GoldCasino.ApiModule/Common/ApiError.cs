namespace GoldCasino.ApiModule.Common;

public record Error(string Code, string Message);
public record SoapApiError(
				string Code,
				string Message,
				int? RemoteCode = null,
				object? Extra = null)
		: Error(Code, Message);