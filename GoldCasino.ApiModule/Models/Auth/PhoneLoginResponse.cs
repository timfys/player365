namespace GoldCasino.ApiModule.Models.Auth;

public class PhoneLoginResponse
{
	public bool Verify { get; set; }
	public string? RedirectUrl { get; set; }
}
