using System.Collections.Generic;
using SmartWinners.Helpers;
using SmartWinners.PaymentSystem.StartAJob;

namespace SmartWinners.Models.Payment;

/// <summary>
/// Request model for completing a deposit
/// </summary>
public class CompleteDepositRequest
{
    /// <summary>
    /// Encrypted data string (d parameter)
    /// </summary>
    public string? DataString { get; set; }
    
    /// <summary>
    /// Alternative data string format (dt parameter)
    /// </summary>
    public string? DataString2 { get; set; }
    
    /// <summary>
    /// Domain type
    /// </summary>
    public DomainType? DomainType { get; set; }
}

/// <summary>
/// Response model for deposit completion
/// </summary>
public class CompleteDepositResponse
{
    public bool Success { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CurrencySymbol { get; set; } = "$";
    public string PaymentType { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request model for checking transaction status
/// </summary>
public class TransactionStatusRequest
{
    /// <summary>
    /// The transaction ID to check
    /// </summary>
    public string TransactionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction type: "3" for Plisio, "5" for Stripe
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
}

/// <summary>
/// Response model for transaction status
/// </summary>
public class TransactionStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public string? Message { get; set; }
    public string? Data { get; set; }
    public string? DomainType { get; set; }
}

/// <summary>
/// Request model for starting a Plisio crypto payment
/// </summary>
public class PlisioPaymentRequest
{
    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Currency code (e.g., "USD", "ILS", "usd", "en")
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Type of payment window
    /// </summary>
    public PaymentWindowType PaymentWindowType { get; set; } = PaymentWindowType.Deposit;
    
    /// <summary>
    /// Optional metadata for payment handlers (e.g., bonus codes)
    /// Example: {"bonus": "welcome"} or {"bonusPercent": "50"}
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Response model for Plisio payment creation
/// </summary>
public class PlisioPaymentResponse
{
    /// <summary>
    /// URL to redirect user to Plisio payment page
    /// </summary>
    public string InvoiceUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction ID from Plisio
    /// </summary>
    public string? TransactionId { get; set; }
}
