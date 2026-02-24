namespace GoldCasino.ApiModule.Models.Auth;

public class AuthStatusResponse
{
    public bool IsAuthenticated { get; set; }
    public string? EntityId { get; set; }
    public string? Username { get; set; }
}
