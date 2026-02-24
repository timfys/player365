namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityVerifyContactInfo
{
  public int EntityId { get; set; }

  public VerificationType VerifyType { get; set; }

  public string? Code { get; set; }

  public Dictionary<string, string>? AdditionalData { get; set; }
}

public class EntityVerifyContactInfoResult
{
  public bool Verified { get; set; }
}

public class EntityVerifyContactInfoResponse : ApiResponse
{
  public const int VerificationCodeAlreadySent = 16;
  public const int UserNotFound = -1;
  public const int InvalidVerificationCode = -14;
  public const int UserHasNoEmail = -439243;

  public bool IsVerificationCodeAlreadySent() => ResultCode == VerificationCodeAlreadySent;
  public bool IsInvalidVerificationCode() => ResultCode == InvalidVerificationCode;
  public bool IsUserHasNoEmail() => ResultCode == UserHasNoEmail;
  public bool IsUserNotFound() => ResultCode == UserNotFound;

}

public enum VerificationType
{
  Phone = 0,
  Email = 1
}