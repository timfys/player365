namespace GoldCasino.ApiModule.Models.Auth;

public class VerifyPhoneRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class VerifyPhoneResponse
{
    public bool IsVerified { get; set; }
    public string Lid { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
