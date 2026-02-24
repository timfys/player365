namespace GoldCasino.ApiModule.Dtos.User;

public class VerifyPhoneDto
{
  public string PhoneNumber { get; set; } = "";
  public string CountryIso { get; set; } = "";
  public string VerificationCode { get; set; } = "";
}

public class VerifyEmailDto
{
  public int EntityId { get; set; }
  public string? Code { get; set; } = "";
}