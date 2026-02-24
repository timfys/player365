namespace SmartWinners.Models.Interfaces;

public interface IVerifyEmailPage
{
    public int ActivePageIndex { get; set; }
    public string Email { get; set; }
    public bool CanAcceptCode { get; set; }
    public string VerificationCode { get; set; }
}
