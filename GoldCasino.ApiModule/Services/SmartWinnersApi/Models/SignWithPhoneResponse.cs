namespace GoldCasino.ApiModule.Services.SmartWinnersApi.Models;

public sealed class SignWithPhone
{
	public required string Phone { get; set; } 
	public required string CountryISO { get; set; }
	public string? Code { get; set; } 
	public int AffiliateEntityId { get; set; } = 0;
}

public sealed class SignWithPhoneResult
{
	public int EntityId { get; set; } = 0;
	public string Lid { get; set; } = string.Empty;
	public bool SmsSent { get; set; }
}

public sealed class SignWithPhoneResponse : ApiResponse
{
	public int EntityId { get; set; } = 0;
	public string Lid { get; set; } = string.Empty;

	// ResultCode values:
	public const int SmsSent = 1;
	public const int PhoneAlreadyRegistered = 2;
	public const int Cooldown = 3;
	public const int MemberRegistered = -13;
	public const int MessageNotFound = -14;
	public const int InvalidCode = -15;
	public const int MobileNotFound = -16;

	// Helper methods
	public bool IsSmsSent() => ResultCode == SmsSent;
	public bool IsPhoneAlreadyRegistered() => ResultCode == PhoneAlreadyRegistered;
	public bool IsCooldown() => ResultCode == Cooldown;
	public bool IsMemberRegistered() => ResultCode == MemberRegistered;
	public bool IsMessageNotFound() => ResultCode == MessageNotFound;
	public bool IsInvalidCode() => ResultCode == InvalidCode;
	public bool IsMobileNotFound() => ResultCode == MobileNotFound;

}
