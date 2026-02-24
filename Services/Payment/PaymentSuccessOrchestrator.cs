using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmartWinners.Services.Payment;

/// <summary>
/// Orchestrates running all payment success handlers
/// </summary>
public class PaymentSuccessOrchestrator
{
    private readonly IEnumerable<IPaymentSuccessHandler> _handlers;
    private readonly ILogger<PaymentSuccessOrchestrator> _logger;

    public PaymentSuccessOrchestrator(
        IEnumerable<IPaymentSuccessHandler> handlers,
        ILogger<PaymentSuccessOrchestrator> logger)
    {
        _handlers = handlers.OrderBy(h => h.Order);
        _logger = logger;
    }

    public async Task ExecuteAsync(PaymentSuccessContext context, CancellationToken cancellationToken = default)
    {
        foreach (var handler in _handlers)
        {
            if (!handler.ShouldHandle(context))
                continue;

            var handlerName = handler.GetType().Name;
            
            try
            {
                _logger.LogDebug("Running payment handler {Handler} for EntityId {EntityId}, TxnId {TxnId}",
                    handlerName, context.EntityId, context.TransactionId);
                
                await handler.HandleAsync(context, cancellationToken);
                
                _logger.LogInformation("Payment handler {Handler} completed for EntityId {EntityId}",
                    handlerName, context.EntityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment handler {Handler} failed for EntityId {EntityId}, TxnId {TxnId}",
                    handlerName, context.EntityId, context.TransactionId);
                // Continue with other handlers - don't fail the whole payment
            }
        }
    }
}
