using System;
using Newtonsoft.Json;
using SmartWinners.Helpers;

namespace SmartWinners.Models.Payment;

public class WithdrawModel
{
    public int PurchasePaymentId { get; set; }

    public double Amount { get; set; }

    public string? ChargedRemark { get; set; }
}

public class WithdrawApiModel
{
    public int purchase_paymentId { get; set; }
    public string CurrencyIso { get; set; }
    public string PaymentId { get; set; }
    public string? PayerNumber { get; set; }
    public string? PayerNumber2 { get; set; }
    public string? PayerNumber3 { get; set; }
    public string? PayerNumber4 { get; set; }
    public string? PayerNumber5 { get; set; }
    public string? PayerNumber6 { get; set; }
    public string? PayerNumber7 { get; set; }
    public DateTime? PaymentDate { get; set; }
    private DateTime? _PaymentDateLocal { get; set; }

    public DateTime? PaymentDateLocal
    {
        get
        {
            if (PaymentDate is null)
                return null;

            if (_PaymentDateLocal.HasValue)
                return _PaymentDateLocal.Value;

            var userTimeZone = WebStorageUtility.GetUserTimeZone();

            _PaymentDateLocal = userTimeZone is null
                ? PaymentDate
                : PaymentDate + userTimeZone.BaseUtcOffset;

            return _PaymentDateLocal.Value;
        }
    }

    public float? PaymentValue { get; set; }

    public string? Parm1 { get; set; }

    [JsonProperty("status")] public WithdrawMethodStatus? Status { get; set; }

    public string WithdrawStatus(bool cardsAndIdVerified)
    {
        switch (Status)
        {
            case WithdrawMethodStatus.Verified:
                return "Withdraw";
            case WithdrawMethodStatus.WaitingForVerification or WithdrawMethodStatus.Declined
                or WithdrawMethodStatus.NotVerified:
                var statusText = "";

                if (cardsAndIdVerified || Status is WithdrawMethodStatus.WaitingForVerification)
                {
                    statusText = "Waiting For Verification";
                }
                else if (Status is WithdrawMethodStatus.NotVerified)
                {
                    statusText = "Not Verified";
                }else if (Status is WithdrawMethodStatus.Declined)
                {
                    statusText = "Declined";
                }
                
                return statusText;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public string WithdrawStatusButton(bool cardsAndIdVerified)
    {
        switch (Status)
        {
            case WithdrawMethodStatus.Verified:
                return "Withdraw";
            case WithdrawMethodStatus.WaitingForVerification or WithdrawMethodStatus.Declined
                or WithdrawMethodStatus.NotVerified:
                var statusText = "";

                if (cardsAndIdVerified || Status is WithdrawMethodStatus.WaitingForVerification)
                {
                    statusText = "Verify again?";
                }
                else if (Status is WithdrawMethodStatus.NotVerified or WithdrawMethodStatus.Declined)
                {
                    statusText = "Verify";
                }
                
                return statusText;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public string WithdrawStatusButtonClass(bool cardsAndIdVerified)
    {        
        switch (Status)
        {
            case WithdrawMethodStatus.Verified:
                return "bank_withdraw";
            case WithdrawMethodStatus.WaitingForVerification or WithdrawMethodStatus.Declined
                or WithdrawMethodStatus.NotVerified:
                var statusText = "";

                if (cardsAndIdVerified || Status is WithdrawMethodStatus.WaitingForVerification)
                {
                    statusText = "bank_checking";
                }
                else if (Status is WithdrawMethodStatus.NotVerified or WithdrawMethodStatus.Declined)
                {
                    statusText = "bank_verify";
                }
                
                return statusText;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    public string WithdrawStatusFormClass(bool cardsAndIdVerified)
    {
        switch (Status)
        {
            case WithdrawMethodStatus.Verified:
                return "verified";
            case WithdrawMethodStatus.WaitingForVerification or WithdrawMethodStatus.Declined
                or WithdrawMethodStatus.NotVerified:
                var statusText = "";

                if (cardsAndIdVerified || Status is WithdrawMethodStatus.WaitingForVerification)
                {
                    statusText = "checking";
                }
                else if(Status is WithdrawMethodStatus.NotVerified or WithdrawMethodStatus.Declined)
                {
                    statusText = "not_verified";
                }
                
                return statusText;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public string WithdrawStatusTextClass(bool cardsAndIdVerified)
    {
        switch (Status)
        {
            case WithdrawMethodStatus.Verified:
                return "status_verified";
            case WithdrawMethodStatus.WaitingForVerification or WithdrawMethodStatus.Declined
                or WithdrawMethodStatus.NotVerified:
                var statusText = "";

                if (cardsAndIdVerified || Status is WithdrawMethodStatus.WaitingForVerification)
                {
                    statusText = "status_checking";
                }
                else if (Status is WithdrawMethodStatus.NotVerified or WithdrawMethodStatus.Declined)
                {
                    statusText = "status_notverified";
                }
                
                return statusText;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum WithdrawMethodStatus
{
    NotVerified,
    Verified,
    WaitingForVerification,
    Declined
}