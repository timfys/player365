using System.Collections.Generic;
using GoldCasino.ApiModule.Services.BusinessApi.Enums;

namespace SmartWinners.Helpers;

public class User : ApiAccessData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Mobile { get; set; }
    public string Country { get; set; }
    
    public string PhonePrefix { get; set; }
    public bool MobileVerified { get; set; }
    
    public decimal BalanceUSD { get; set; }
    
    public decimal BalanceLocal { get; set; }
    
    public decimal UserAffiliateEarnings { get; set; }
    public int UserAffiliateReferred { get; set; }
    
    public decimal TotalWinningsUsd { get; set; }
    public decimal TotalWithdrawAmountUsd { get; set; }

    public int VerificationDocFileId { get; set; } = 0;
    public int ProfileImageId { get; set; }

    public string Lid
    {
        get
        {
            WebStorageUtility.TryGetString("Lid", out var lid);
            return lid;
        }
    }

    public IdDocVerificationState IdDocVerificationState { get; set; }
    public int AffiliateId { get; set; }
    public string ZipCode { get; set; }
    public string Address { get; set; }
    public string State { get; set; }
    public string City { get; set; }
    
    public decimal VirtualBalance { get; set; }
    public decimal VirtualBalanceLocal { get; set; }
    public bool EmailVerified { get; set; }
    public void SetVirtualBalance(decimal virtualBalance)
    {
        VirtualBalance = virtualBalance;
        VirtualBalanceLocal = CurrencyHelper.GetCurrency().ExchangeRate * virtualBalance;
    }
    
    public string SuspendType { get; set; }

    public bool IsSuspended
    {
        get
        {
            return "1".Equals(SuspendType);
        }
    }

    public decimal Player1TotalWinnings { get; set; }
    public string Company { get; set; }
}
