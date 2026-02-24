namespace SmartWinners.Models.Interfaces;

public interface ISignupVerification
{
    public int ActivePageIndex { get; set; }

    public string PhoneCountry { get; set; }
    public string PhonePrefix { get; set; }
    public string Phone { get; set; }

    public bool CanAcceptCode { get; set; }
    public string VerificationCode { get; set; }
}