namespace SmartWinners.Models.Interfaces;

public interface ISignInPage
{
    public bool? IsModel { get; set; }
    public int ActivePageIndex { get; set; }
    public string Country { get; set; }
    public string PhonePrefix { get; set; }
    public string PhoneOrEmail { get; set; }
    public string Password { get; set; }
    /* Found first name */
    public string FirstName { get; set; }
    public string _BtnBackToPage0 { get; set; }

    public bool IsActionToPage1 { get; set; }

    public bool IsActionBackToPage0 => _BtnBackToPage0 != null;
    
    public bool FromLanding { get; set; }
    
    public string AffiliateData { get; set; }
    
    public int LotteryIdFromLanding { get; set; }
    public bool Continue { get; set; }
    
    public void ClearActions()
    {
        _BtnBackToPage0 = null;
    }
}
