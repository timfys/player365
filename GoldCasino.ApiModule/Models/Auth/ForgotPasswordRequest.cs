using GoldCasino.ApiModule.Services.BusinessApi.Models;

namespace GoldCasino.ApiModule.Models.Auth;
public class ForgotPasswordRequest
{
	public string Username { get; set; }
	public RemindKind RemindKind { get; set; }
	public string? CountryIso { get; set; }
	public string? HostBase { get; set; }
	public string? Token { get; set; }
	public string? NewPassword { get; set; }
	public string? Language { get; set; }
}
