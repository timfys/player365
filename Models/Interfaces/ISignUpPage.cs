namespace SmartWinners.Models.Interfaces;

public interface ISignUpPage
{
    //public bool? IsModel { get; set; }
    //public int ActivePageIndex { get; set; }
    public string PhoneCountry { get; set; }
    public string PhonePrefix { get; set; }
    public string Phone { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Country { get; set; }
    public string Password { get; set; }
    public string Password2 { get; set; }
    //public bool CanAcceptCode { get; set; }
    //public string VerificationCode { get; set; }
}
