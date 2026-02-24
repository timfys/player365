using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartWinners.PaymentSystem.StartAJob;

namespace SmartWinners.Services.Payment;

/// <summary>
/// Context passed to payment success handlers
/// </summary>
public class PaymentSuccessContext
{
    public required int EntityId { get; init; }
    public required int RecordId { get; init; }
    public required PaymentWindowType PaymentType { get; init; }
    public required string TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    
    /// <summary>
    /// Custom metadata encoded in the order (e.g., bonus codes, voucher info)
    /// </summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Interface for handlers that run after successful payment
/// </summary>
public interface IPaymentSuccessHandler
{
    /// <summary>
    /// Order of execution (lower = earlier)
    /// </summary>
    int Order => 0;
    
    /// <summary>
    /// Whether this handler should run for the given context
    /// </summary>
    bool ShouldHandle(PaymentSuccessContext context);
    
    /// <summary>
    /// Execute the handler logic
    /// </summary>
    Task HandleAsync(PaymentSuccessContext context, CancellationToken cancellationToken = default);
}
