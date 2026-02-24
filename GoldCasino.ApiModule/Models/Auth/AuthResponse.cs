namespace GoldCasino.ApiModule.Models.Auth;

public class AuthResponse
{
    public bool Success { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Username { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
}
