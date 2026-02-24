namespace GoldCasino.ApiModule.Models.Auth;

public class AuthLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Language { get; set; } = "US";
}
