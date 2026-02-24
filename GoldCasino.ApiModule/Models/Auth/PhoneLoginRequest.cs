namespace GoldCasino.ApiModule.Models.Auth;

public class PhoneLoginRequest
{
	public string Phone { get; set; } = string.Empty;
	public int AffiliateEntityId { get; set; } = 0;
}
