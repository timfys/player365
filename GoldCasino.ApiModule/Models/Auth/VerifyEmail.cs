namespace GoldCasino.ApiModule.Models.Auth;

public class VerifyEmail
{

}

public class VerifyEmailRequest
{
  public string Email { get; set; } = null!;

  public string Code { get; set; } = null!;
}


