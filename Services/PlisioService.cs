using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusinessApi;
using GoldCasino.ApiModule.Configuration;
using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.SmartWinnersApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Plisio.ApiClient;
using SmartWinners.Helpers;
using SmartWinners.Models;
using SmartWinners.Models.Payment;
using SmartWinners.PaymentSystem.StartAJob;
using SmartWinners.Services.Payment;

namespace SmartWinners.Services;

/// <summary>
/// Service for handling Plisio cryptocurrency payment operations
/// </summary>
public class PlisioService(
    IPlisioApiClient plisioClient,
    BusinessApiService businessApiService,
    IBusinessApiService businessApi,
    ISmartWinnersApiService smartWinnersApiService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PlisioService> logger,
    IOptions<PlisioOptions> options,
    PaymentSuccessOrchestrator paymentSuccessOrchestrator)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly PlisioOptions _options = options.Value;

    /// <summary>
    /// Creates a Plisio invoice for crypto payment using the generated API client
    /// </summary>
    public async Task<PlisioInvoiceResult> CreateInvoiceAsync(PlisioInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available");

        var url = !string.IsNullOrEmpty(_options.CallbackUrl)
            ? _options.CallbackUrl
            : $"https://{context.Request.Host}";

        // Log the payment attempt first1
        var recordId = await LogPaymentAsync(request.Type, "USD", request.UsdAmount, "",
            "Plisio transaction", request.PayerEntityId);

        // Create unique order number: recordId_entityId_paymentType[_metadata]
        // Metadata is base64 encoded JSON for passing to success handlers
        var orderNumber = BuildOrderNumber(recordId, request.PayerEntityId, request.Type, request.Metadata);

        var transactionName = request.Type switch
        {
            PaymentWindowType.Deposit => "Deposit",
            PaymentWindowType.Lottery => "Lottery",
            PaymentWindowType.Syndicate => "Syndicate",
            _ => "Payment"
        };

        // Build callback URLs per Plisio documentation:
        // - callback_url: Status URL for invoice updates (webhook, receives POST with JSON)
        // - success_callback_url: Server-to-server callback when invoice is paid (POST)
        // - fail_callback_url: Server-to-server callback when invoice fails (POST)
        // - success_invoice_url: "To the site" button link for paid invoice (user redirect)
        // - fail_invoice_url: "To the site" button link for unpaid invoice (user redirect)

        // Main webhook URL - receives ALL status updates (new, pending, completed, expired, etc.)
        // Using json=true ensures we get JSON response format instead of plain text
        var callbackUrl = $"{url}/WebHook/Plisio?json=true";

        // Success/fail server callbacks - use the same webhook since it handles all statuses
        // These are redundant with callback_url but Plisio may call them separately
        var successCallbackUrl = callbackUrl;
        var failCallbackUrl = callbackUrl;

        // User redirect URLs after payment (where "To the site" button goes)
        var successInvoiceUrl = BuildSuccessRedirectUrl(request, orderNumber);
        var failInvoiceUrl = $"{request.RedirectFailUrl}/{orderNumber}?tT=3";

        logger.LogInformation(
            "Creating Plisio invoice for EntityId: {EntityId}, Amount: {Amount} USD, OrderNumber: {OrderNumber}",
            request.PayerEntityId, request.UsdAmount, orderNumber);

        try
        {
            // Use the generated API client instead of raw HTTP
            var response = await plisioClient.CreateInvoiceAsync(
                currency: null, // Let user choose from allowed cryptocurrencies
                amount: null, // We use source_amount/source_currency for fiat conversion
                source_currency: "USD",
                source_amount: $"{request.UsdAmount:0.00}",
                allowed_psys_cids: _options.AllowedCryptocurrencies, // e.g., "BTC,ETH,USDT_TRX"
                order_name: $"{transactionName} {orderNumber}",
                order_number: orderNumber,
                description: $"{transactionName} payment for {request.PayerEntityId}",
                callback_url: callbackUrl,
                email: $"{request.PayerEntityId}@smart-winners.com",
                language: "en_US",
                plugin: null,
                version: null,
                redirect_to_invoice: null,
                expire_min: _options.DefaultExpireMinutes.ToString(),
                success_callback_url: successCallbackUrl,
                fail_callback_url: failCallbackUrl,
                success_invoice_url: successInvoiceUrl,
                fail_invoice_url: failInvoiceUrl,
                return_existing: 0,
                cancellationToken: cancellationToken);

            if (response?.Data == null)
            {
                logger.LogWarning("Plisio API returned null response");
                return PlisioInvoiceResult.Failure("Plisio API returned empty response", orderNumber, recordId);
            }

            logger.LogInformation(
                "Plisio invoice created successfully: TxnId={TxnId}, InvoiceUrl={InvoiceUrl}",
                response.Data.Txn_id, response.Data.Invoice_url?.ToString());

            return PlisioInvoiceResult.Success(
                txnId: response.Data.Txn_id,
                invoiceUrl: response.Data.Invoice_url?.ToString() ?? string.Empty,
                orderNumber: orderNumber,
                recordId: recordId,
                walletHash: response.Data.Wallet_hash,
                amount: response.Data.Amount,
                currency: response.Data.Currency);
        }
        catch (ApiException<ErrorResponseDto> ex)
        {
            logger.LogError(ex, "Plisio API error creating invoice: {Message}", ex.Result?.Data?.Message);
            return PlisioInvoiceResult.Failure(ex.Result?.Data?.Message ?? ex.Message, orderNumber, recordId);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Plisio API exception: {Message}", ex.Message);
            return PlisioInvoiceResult.Failure(ex.Message, orderNumber, recordId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating Plisio invoice");
            return PlisioInvoiceResult.Failure(ex.Message, orderNumber, recordId);
        }
    }

    /// <summary>
    /// Gets invoice details from Plisio
    /// </summary>
    public async Task<PlisioInvoiceDetails?> GetInvoiceDetailsAsync(string invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await plisioClient.GetInvoiceDetailsAsync(invoiceId, cancellationToken);

            if (response?.Data?.Invoice == null)
                return null;

            var invoice = response.Data.Invoice;
            return new PlisioInvoiceDetails
            {
                Id = invoice.Id,
                // Status is typically obtained from operations, not invoice details
                // For now we'll leave it null and get it from the polling mechanism
                Status = null,
                Currency = invoice.Currency,
                Amount = invoice.Amount,
                WalletHash = invoice.Wallet_hash,
                SourceAmount = invoice.Params?.Source_amount,
                SourceCurrency = invoice.Source_currency
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting invoice details for {InvoiceId}", invoiceId);
            return null;
        }
    }

    /// <summary>
    /// Polls Plisio API to check transaction status
    /// </summary>
    public async Task<PlisioTransactionStatus> GetTransactionStatusAsync(
        string transactionId,
        int maxAttempts = 20,
        int delayMs = 5000,
        CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (attempt > 0)
            {
                await Task.Delay(delayMs, cancellationToken);
            }

            try
            {
                // Get invoice details to check status
                var details = await GetInvoiceDetailsAsync(transactionId, cancellationToken);

                if (details == null)
                {
                    logger.LogDebug("Transaction {TransactionId} not found, attempt {Attempt}/{MaxAttempts}",
                        transactionId, attempt + 1, maxAttempts);
                    continue;
                }

                var status = details.Status?.ToLowerInvariant();

                // Check for terminal states
                if (status == "completed")
                    return new PlisioTransactionStatus { Status = "completed", IsComplete = true, Details = details };

                if (status == "mismatch")
                    return new PlisioTransactionStatus { Status = "mismatch", IsComplete = true, Details = details };

                if (status == "expired")
                    return new PlisioTransactionStatus { Status = "expired", IsComplete = false, Details = details };

                if (status == "error")
                    return new PlisioTransactionStatus { Status = "error", IsComplete = false, Details = details };

                if (status == "cancelled")
                    return new PlisioTransactionStatus { Status = "cancelled", IsComplete = false, Details = details };

                logger.LogDebug("Transaction {TransactionId} status: {Status}, attempt {Attempt}/{MaxAttempts}",
                    transactionId, status, attempt + 1, maxAttempts);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error checking transaction status, attempt {Attempt}/{MaxAttempts}",
                    attempt + 1, maxAttempts);
            }
        }

        return new PlisioTransactionStatus { Status = "timeout", IsComplete = false };
    }

    /// <summary>
    /// Processes webhook payment notification from Plisio
    /// </summary>
    public async Task<PlisioPaymentResult> ProcessWebhookPaymentAsync(PlisioWebhookRequest model)
    {
        var result = new PlisioPaymentResult();

        try
        {
            // Parse order_number: format is "recordId_entityId_paymentType[_metadata]"
            var (recordId, entityId, paymentType, metadata) = ParseOrderNumber(model.order_number);
            if (recordId == 0)
            {
                logger.LogError("Invalid order_number format: {OrderNumber}", model.order_number);
                return result.WithError("Invalid order number format");
            }

            result.RecordId = recordId;
            result.EntityId = entityId;
            result.PaymentType = paymentType;

            // Check if transaction already exists
            if (await CheckIfTransactionExistsAsync(model.txn_id))
            {
                logger.LogInformation("Transaction {TxnId} already processed", model.txn_id);
                result.AlreadyProcessed = true;
                return result;
            }

            // Process the payment
            var paymentResult = await ProcessPaymentAsync(entityId, model, paymentType, recordId);
            result.Success = paymentResult.Success;
            result.PaymentInfo = paymentResult.PaymentInfo;

            if (result.Success)
            {
                await UpdateSuccessfulPaymentLogAsync(recordId);
                logger.LogInformation(
                    "Successfully processed Plisio payment for EntityId: {EntityId}, TxnId: {TxnId}, Amount: {Amount} {Currency}",
                    entityId, model.txn_id, model.source_amount, model.source_currency);

                // Recalculate user balance after successful payment
                await RecalculateUserBalanceAsync(entityId);

                // Run payment success handlers (bonuses, vouchers, notifications, etc.)
                await RunPaymentSuccessHandlersAsync(entityId, recordId, paymentType, model, metadata);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Plisio webhook payment for TxnId: {TxnId}", model.txn_id);
            return result.WithError(ex.Message);
        }
    }

    private async Task RunPaymentSuccessHandlersAsync(
        int entityId,
        int recordId,
        PaymentWindowType paymentType,
        PlisioWebhookRequest model,
        Dictionary<string, string> metadata)
    {
        try
        {
            var context = new PaymentSuccessContext
            {
                EntityId = entityId,
                RecordId = recordId,
                PaymentType = paymentType,
                TransactionId = model.txn_id,
                Amount = model.source_amount,
                Currency = model.source_currency,
                Metadata = metadata
            };

            await paymentSuccessOrchestrator.ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            // Log but don't fail the payment - handlers are non-critical
            logger.LogError(ex, "Error running payment success handlers for TxnId: {TxnId}", model.txn_id);
        }
    }

    private async Task RecalculateUserBalanceAsync(int entityId)
    {
        try
        {
            // Get user credentials from BusinessApi
            var result = await businessApi.EntityFindAsync(new EntityFind
            {
                Fields = FieldHelper<UserAccessDto>.Fields,
                Filter = new() { { "entityId", $"{entityId}" } }
            });

            if (!result.IsSuccess || result.Value?.Entities.Count == 0)
            {
                logger.LogWarning("Could not find entity {EntityId} for balance recalculation", entityId);
                return;
            }

            var entity = result.Value?.Entities.First();
            var userAccess = EntityMapper.MapTo<UserAccessDto>(entity);

            var accessData = new UserApiAccess(userAccess.Id, userAccess.Username, userAccess.PasswordHash);

            // Recalculate the balance
            await smartWinnersApiService.EntityBalanceCalcAsync(accessData);
            logger.LogInformation("Successfully recalculated balance for EntityId: {EntityId}", entityId);
        }
        catch (Exception ex)
        {
            // Log but don't fail - balance recalculation is non-critical
            logger.LogError(ex, "Error recalculating balance for EntityId: {EntityId}", entityId);
        }
    }

    /// <summary>
    /// Builds order number with optional metadata encoding
    /// Format: recordId_entityId_paymentType[_base64Metadata]
    /// </summary>
    private static string BuildOrderNumber(int recordId, int entityId, PaymentWindowType paymentType, Dictionary<string, string>? metadata)
    {
        var baseOrder = $"{recordId}_{entityId}_{Convert.ToInt32(paymentType)}";

        if (metadata == null || metadata.Count == 0)
            return baseOrder;

        // Encode metadata as URL-safe base64 (Plisio order_number has length limits)
        var json = System.Text.Json.JsonSerializer.Serialize(metadata);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .Replace('+', '-')  // URL-safe
            .Replace('/', '_')  // URL-safe
            .TrimEnd('=');      // Remove padding

        return $"{baseOrder}_{base64}";
    }

    /// <summary>
    /// Parses order number and extracts metadata
    /// </summary>
    private (int RecordId, int EntityId, PaymentWindowType PaymentType, Dictionary<string, string> Metadata) ParseOrderNumber(string orderNumber)
    {
        var parts = orderNumber.Split('_');
        if (parts.Length < 3)
            return (0, 0, PaymentWindowType.Deposit, new Dictionary<string, string>());

        if (!int.TryParse(parts[0], out var recordId) ||
            !int.TryParse(parts[1], out var entityId) ||
            !Enum.TryParse<PaymentWindowType>(parts[2], out var paymentType))
        {
            return (0, 0, PaymentWindowType.Deposit, new Dictionary<string, string>());
        }

        var metadata = new Dictionary<string, string>();

        // If there's a 4th part, it's base64 encoded metadata
        if (parts.Length > 3)
        {
            try
            {
                var base64 = parts[3]
                    .Replace('-', '+')  // Restore from URL-safe
                    .Replace('_', '/'); // Restore from URL-safe

                // Add padding if needed
                var padding = (4 - base64.Length % 4) % 4;
                base64 = base64.PadRight(base64.Length + padding, '=');

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
                metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                    ?? new Dictionary<string, string>();
            }
            catch (Exception)
            {
                // Invalid metadata, ignore
            }
        }

        return (recordId, entityId, paymentType, metadata);
    }

    /// <summary>
    /// Verifies the webhook signature from Plisio using HMAC-SHA1
    /// </summary>
    public bool VerifyWebhookSignature(string jsonData, string verifyHash)
    {
        // Minify JSON (remove whitespace outside of strings)
        var minifiedJson = Regex.Replace(jsonData, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_options.SecretKey));
        var dataBytes = Encoding.UTF8.GetBytes(minifiedJson);
        var hashBytes = hmac.ComputeHash(dataBytes);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hash == verifyHash;
    }

    /// <summary>
    /// Checks if a transaction has already been processed
    /// </summary>
    public async Task<bool> CheckIfTransactionExistsAsync(string transactionId)
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var apiRequest = new Sales_Orders_Payments_GetRequest
        {
            ol_EntityID = config.ol_EntityId,
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            BusinessId = config.BusinessId,
            FilterFields = new[] { "transactionID" },
            FilterValues = new[] { transactionId }
        };

        var apiResponse = await Task.Run(() => client.Sales_Orders_Payments_Get(apiRequest));
        return !apiResponse.@return.Equals("[]");
    }

    public async Task SendFailedChargeNotificationAsync(int entityId, int messageId = 110)
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var apiRequest = new Outgoing_addRequest
        {
            ol_EntityID = config.ol_EntityId,
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            EntityIds = new[] { entityId },
            MessageType = 6,
            MessageID = messageId
        };

        await Task.Run(() => client.Outgoing_add(apiRequest));
    }

    #region Private Methods

    private string BuildSuccessRedirectUrl(PlisioInvoiceRequest request, string orderNumber)
    {
        var redirectSuccessUrl = $"{request.RedirectSuccessUrl}/{orderNumber}?tT=3";

        if (request.Type == PaymentWindowType.Deposit)
        {
            var currency = WebStorageUtility.GetUserCurrencyDetails();
            var currencyIso = currency?.CurrencyIso ?? "USD";
            var currencySymbol = currency?.Symbol ?? "$";
            var encryptedData = CryptoUtility.EncryptString(
                $"{request.DepositDisplayAmount:0.00}:;:{(request.IsUsdDeposit ? "USD" : currencyIso)}:;:2:;:{(request.IsUsdDeposit ? "$" : currencySymbol)}")
                .Replace("+", " ");
            redirectSuccessUrl += $"&d={encryptedData}";
        }

        return redirectSuccessUrl;
    }

    private async Task<(bool Success, PaymentInfo? PaymentInfo)> ProcessPaymentAsync(
        int entityId, PlisioWebhookRequest model, PaymentWindowType type, int recordId)
    {
        try
        {
            var apiConfig = EnvironmentHelper.SmartWinnersApiConfiguration;
            var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

            var request = new Sales_Orders_Payment_UpdateRequest
            {
                ol_Username = apiConfig.ol_UserName,
                ol_Password = apiConfig.ol_Password,
                ol_EntityID = apiConfig.ol_EntityId,
                EntityId = entityId,
                BusinessId = 1,
                order_paymentId = 0,
                NamesArray = new[]
                {
                    "PaymentID", "OrderId", "PaymentValue", "Employee_entityId", "currencyIso", "PayerName",
                    "PayerNumber", "PayerNumber3", "PayerDate", "transactionID", "ChargedRemark", "status", "ChargedDate"
                },
                ValuesArray = new[]
                {
                    "5", "-2", $"{Math.Round(model.source_amount, 2):0.00}", "4", "USD", "", "", "",
                    "2055-10-10", model.txn_id, $"Plisio - {model.currency} {model.amount}", "1", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                },
            };

            var response = await client.Sales_Orders_Payment_UpdateAsync(request);
            var paymentInfo = JsonConvert.DeserializeObject<PaymentInfo>(response.@return);

            if (paymentInfo?.IsSuccess() == true)
            {
                return (true, paymentInfo);
            }

            await LogPaymentAsync(type, "USD", Math.Round(model.amount, 2), paymentInfo?.ResultMessage ?? "Unknown error",
                $"Terminal Id: 5 \n Payment data: {JsonConvert.SerializeObject(model)}", entityId, recordId);

            return (false, paymentInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment for EntityId: {EntityId}", entityId);

            await LogPaymentAsync(type, "USD", Math.Round(model.amount, 2), $"{ex.Message}",
                $"Terminal Id: 5 \n Payment data: {JsonConvert.SerializeObject(model)}", entityId, recordId);

            return (false, null);
        }
    }

    private async Task<int> LogPaymentAsync(PaymentWindowType paymentType, string currency, decimal paymentSum,
        string paymentError, string paymentDetails, int entityId, int? recordId = null)
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var paymentTypeStr = paymentType switch
        {
            PaymentWindowType.Deposit => "6",
            PaymentWindowType.Lottery => "4",
            PaymentWindowType.Syndicate or PaymentWindowType.SyndicatePromotion => "5",
            PaymentWindowType.CardVerification => "9",
            _ => "6"
        };

        var fieldsDict = new Dictionary<string, string>
        {
            { "CustomField141", paymentError },
            { "CustomField140", paymentTypeStr },
            { "CustomField139", paymentDetails },
            { "CustomField138", $"{paymentSum:0.00}" },
            { "CustomField137", DateTime.Now.ToString("yyyy-MM-dd h:mm:ss") },
            { "ParentRecordID", $"{entityId}" }
        };

        if (!string.IsNullOrEmpty(currency))
        {
            fieldsDict.Add("CustomField190", currency);
        }

        var apiRequest = new CustomFields_Tables_UpdateRequest
        {
            ol_EntityID = config.ol_EntityId,
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            TableID = 136,
            NamesArray = [.. fieldsDict.Keys],
            ValuesArray = [.. fieldsDict.Values]
        };

        if (recordId is > 0)
        {
            apiRequest.RecordID = recordId.Value;
        }

        var resp = await client.CustomFields_Tables_UpdateAsync(apiRequest);
        return JsonConvert.DeserializeObject<CustomTableEntry>(resp.@return)?.RecordId ?? 0;
    }

    private async Task UpdateSuccessfulPaymentLogAsync(int recordId)
    {
        var config = EnvironmentHelper.BusinessApiConfiguration;
        var client = config.InitClient();

        var apiRequest = new CustomFields_Tables_UpdateRequest
        {
            ol_EntityID = config.ol_EntityId,
            ol_Username = config.ol_UserName,
            ol_Password = config.ol_Password,
            TableID = 136,
            RecordID = recordId,
            NamesArray = new[] { "CustomField141", "IsDeleted" },
            ValuesArray = new[] { "OK", "1" }
        };

        await Task.Run(() => client.CustomFields_Tables_Update(apiRequest));
    }

    private class CustomTableEntry
    {
        public int RecordId { get; set; }
    }

    #endregion
}

#region DTOs

/// <summary>
/// Result of creating a Plisio invoice
/// </summary>
public class PlisioInvoiceResult
{
    public bool IsSuccess { get; private set; }
    public string? TxnId { get; private set; }
    public string? InvoiceUrl { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public int RecordId { get; private set; }
    public string? WalletHash { get; private set; }
    public string? Amount { get; private set; }
    public string? Currency { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static PlisioInvoiceResult Success(
        string txnId, string invoiceUrl, string orderNumber, int recordId,
        string? walletHash = null, string? amount = null, string? currency = null)
    {
        return new PlisioInvoiceResult
        {
            IsSuccess = true,
            TxnId = txnId,
            InvoiceUrl = invoiceUrl,
            OrderNumber = orderNumber,
            RecordId = recordId,
            WalletHash = walletHash,
            Amount = amount,
            Currency = currency
        };
    }

    public static PlisioInvoiceResult Failure(string errorMessage, string orderNumber, int recordId)
    {
        return new PlisioInvoiceResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            OrderNumber = orderNumber,
            RecordId = recordId
        };
    }
}

/// <summary>
/// Invoice details from Plisio
/// </summary>
public class PlisioInvoiceDetails
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public string? Currency { get; set; }
    public string? Amount { get; set; }
    public string? WalletHash { get; set; }
    public string? SourceAmount { get; set; }
    public string? SourceCurrency { get; set; }
}

/// <summary>
/// Transaction status check result
/// </summary>
public class PlisioTransactionStatus
{
    public string Status { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public PlisioInvoiceDetails? Details { get; set; }
}

/// <summary>
/// Result of processing a webhook payment
/// </summary>
public class PlisioPaymentResult
{
    public bool Success { get; set; }
    public bool AlreadyProcessed { get; set; }
    public int RecordId { get; set; }
    public int EntityId { get; set; }
    public PaymentWindowType PaymentType { get; set; }
    public PaymentInfo? PaymentInfo { get; set; }
    public string? ErrorMessage { get; set; }

    public PlisioPaymentResult WithError(string message)
    {
        ErrorMessage = message;
        Success = false;
        return this;
    }
}

#endregion
