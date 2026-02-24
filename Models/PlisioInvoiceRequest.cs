using System.Collections.Generic;
using SmartWinners.PaymentSystem.StartAJob;

namespace SmartWinners.Models;

public class PlisioInvoiceRequest
{
    public string RedirectSuccessUrl { get; set; }

    public string RedirectFailUrl { get; set; }

    public decimal UsdAmount { get; set; }
    public decimal DepositDisplayAmount { get; set; }
    public bool IsUsdDeposit { get; set; }

    public PaymentWindowType Type { get; set; }

    public int PayerEntityId { get; set; }
    
    /// <summary>
    /// Optional metadata to pass to payment success handlers
    /// Examples: {"bonus": "welcome"}, {"bonusPercent": "50"}, {"voucherCode": "SUMMER50"}
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}