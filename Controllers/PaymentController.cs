using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartWinners.Helpers;
using SmartWinners.Models;
using SmartWinners.Models.BusinessAPI.Sales;
using SmartWinners.Models.Payment;
using SmartWinners.PaymentSystem.StartAJob;
using SmartWinners.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Controllers;

[Route("Payment")]
public class PaymentController(
	UmbracoHelper umbracoHelper,
	BusinessApiService businessApiService,
	StripeService stripeService,
	PlisioService plisioService,
	LadderBonusService ladderBonusService,
	ILogger<PaymentController> logger) : Controller
{
	#region Deposit Completion & Status

	// /// <summary>
	// /// Completes a deposit callback and returns payment data
	// /// </summary>
	// [HttpPost("complete/purchase-coins")]
	// public async Task<IActionResult> CompleteDepositCallback([FromBody] CompleteDepositRequest request, CancellationToken cancellationToken)
	// {
	// 	try
	// 	{
	// 		string[] paymentData;
			
	// 		if (!string.IsNullOrEmpty(request.DataString2))
	// 		{
	// 			var parts = request.DataString2.Split("---").ToList();
	// 			parts.Add(CurrencyHelper.GetCurrency().Symbol);
	// 			paymentData = [.. parts];
	// 		}
	// 		else if (!string.IsNullOrEmpty(request.DataString))
	// 		{
	// 			paymentData = CryptoUtility.DecryptString(request.DataString).Split(":;:");
	// 		}
	// 		else
	// 		{
	// 			return BadRequest("Missing payment data");
	// 		}

	// 		if (paymentData.Length < 1)
	// 		{
	// 			return BadRequest("Invalid payment data format");
	// 		}

	// 		var amount = decimal.Parse(paymentData[0]);
	// 		var currency = paymentData.Length > 1 ? paymentData[1] : "USD";
	// 		var paymentType = paymentData.Length > 2 ? paymentData[2] : "unknown";
	// 		var currencySymbol = paymentData.Length > 3 ? paymentData[3] : "$";

	// 		logger.LogInformation("Deposit completed: Amount={Amount}, Currency={Currency}, Type={Type}", 
	// 			amount, currency, paymentType);

	// 		return Ok(new CompleteDepositResponse
	// 		{
	// 			Success = true,
	// 			Amount = amount,
	// 			Currency = currency,
	// 			CurrencySymbol = currencySymbol,
	// 			PaymentType = paymentType
	// 		});
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		logger.LogError(ex, "Error completing deposit callback");
	// 		return BadRequest("Failed to process deposit completion");
	// 	}
	// }

	/// <summary>
	/// Checks the status of a payment transaction (Plisio or Stripe)
	/// </summary>
	// [HttpPost("transaction/status")]
	// public async Task<IActionResult> GetTransactionStatus([FromBody] TransactionStatusRequest request, CancellationToken cancellationToken)
	// {
	// 	if (string.IsNullOrEmpty(request.TransactionId))
	// 	{
	// 		return BadRequest("Transaction ID is required");
	// 	}

	// 	var transactionId = request.TransactionId.Replace("-", "_");

	// 	switch (request.TransactionType)
	// 	{
	// 		case "3": // Plisio
	// 			return await HandlePlisioTransactionStatusAsync(transactionId, cancellationToken);

	// 		// case "5": // Stripe
	// 		// 	return await HandleStripeTransactionStatusAsync(transactionId);

	// 		default:
	// 			return BadRequest("Undefined transaction type");
	// 	}
	// }

	// private async Task<IActionResult> HandlePlisioTransactionStatusAsync(string transactionId, CancellationToken cancellationToken)
	// {
	// 	var statusResult = await plisioService.GetTransactionStatusAsync(transactionId, cancellationToken: cancellationToken);

	// 	if (statusResult.IsComplete)
	// 	{
	// 		// await PaymentHelper.RecalculateUserBalance(null, true, HttpContext);
			
	// 		logger.LogInformation("Plisio transaction {TransactionId} completed with status: {Status}", 
	// 			transactionId, statusResult.Status);
			
	// 		return Ok(new TransactionStatusResponse
	// 		{
	// 			Status = statusResult.Status,
	// 			IsComplete = true,
	// 			Message = "Payment completed successfully"
	// 		});
	// 	}

	// 	// Handle failed/pending transactions
	// 	if (!WebStorageUtility.TryGetString(WebStorageUtility.FailedChargeSentByEmail, out _))
	// 	{
	// 		var transactionValues = transactionId.Split("_");
	// 		if (transactionValues.Length > 1 && int.TryParse(transactionValues[1], out var entityId))
	// 		{
	// 			await plisioService.SendFailedChargeNotificationAsync(entityId, 110);
	// 			WebStorageUtility.SetString(WebStorageUtility.FailedChargeSentByEmail, "1",
	// 				WebStorageUtility.GetUserDateTime() + TimeSpan.FromDays(1));
	// 		}
	// 	}

	// 	logger.LogWarning("Plisio transaction {TransactionId} failed with status: {Status}", 
	// 		transactionId, statusResult.Status);

	// 	return BadRequest(new TransactionStatusResponse
	// 	{
	// 		Status = statusResult.Status,
	// 		IsComplete = false,
	// 		Message = $"Transaction {statusResult.Status}"
	// 	});
	// }

	// private async Task<IActionResult> HandleStripeTransactionStatusAsync(string transactionId)
	// {
	// 	var status = StripeService.PoolTransaction(transactionId, out var data);

	// 	var domainType = data.Metadata["DT"];
	// 	Response.Headers.Append("dtt", $"{domainType}");

	// 	if (status is "complete" or "succeeded")
	// 	{
	// 		await PaymentHelper.RecalculateUserBalance(null, true, HttpContext);

	// 		var ogUrl = data.Metadata["Ou"];
	// 		var name = DomainsHelper.GetDomainName(Enum.Parse<DomainType>(domainType));
	// 		var currency = CurrencyHelper.GetCurrency();
	// 		var successData = CryptoUtility.EncryptString(
	// 			$"{Convert.ToDecimal(data.Amount) / 100:0.00}:;:{data.Currency}:;:2:;:{(!data.Currency.Equals("USD", StringComparison.OrdinalIgnoreCase) ? currency.Symbol : "$")}");
			
	// 		Response.Headers.Append("data", successData);
	// 		Response.Headers.Append("dt", name);

	// 		if (!string.IsNullOrEmpty(ogUrl))
	// 		{
	// 			Response.Headers.Append("ogUrl", HttpUtility.UrlEncode(CryptoUtility.DecryptString(HttpUtility.UrlDecode(ogUrl))));
	// 		}

	// 		logger.LogInformation("Stripe transaction {TransactionId} completed successfully", transactionId);

	// 		return Ok(new TransactionStatusResponse
	// 		{
	// 			Status = status.ToLower(),
	// 			IsComplete = true,
	// 			Message = "Payment completed successfully",
	// 			Data = successData,
	// 			DomainType = name
	// 		});
	// 	}

	// 	logger.LogWarning("Stripe transaction {TransactionId} failed with status: {Status}", transactionId, status);

	// 	return BadRequest(new TransactionStatusResponse
	// 	{
	// 		Status = status.ToLower(),
	// 		IsComplete = false,
	// 		Message = $"Transaction {status}"
	// 	});
	// }

	#endregion

	#region Plisio Payment
	
	[Authorize]
	[HttpPost("Plisio")]
	public async Task<IActionResult> StartPlisioCryptoPayment([FromBody] PlisioPaymentRequest request)
	{
		var user = HttpContext.User.ToUserApiAccess();
		WebStorageUtility.SetString(WebStorageUtility.AfterPaymentFlag, "1");

		var amount = request.Amount;
		var isUsd = request.Currency.Equals("usd", StringComparison.OrdinalIgnoreCase) || 
		            request.Currency.Equals("en", StringComparison.OrdinalIgnoreCase);
		
		if (!isUsd)
		{
			var currencyInfo = CurrencyHelper.GetCurrency();
			amount /= currencyInfo.ExchangeRate;
		}

		var redirectUrl = request.PaymentWindowType switch
		{
			PaymentWindowType.Deposit => $"https://{HttpContext.Request.Host}{WebStorageUtility.GetUserLangIso(true)}/purchase-coins",
			_ => throw new ArgumentOutOfRangeException(nameof(request.PaymentWindowType), request.PaymentWindowType, "Unsupported payment type")
		};

		var invoiceRequest = new PlisioInvoiceRequest
		{
			Type = request.PaymentWindowType,
			PayerEntityId = user.EntityId,
			UsdAmount = amount,
			DepositDisplayAmount = request.Amount,
			IsUsdDeposit = isUsd,
			RedirectSuccessUrl = redirectUrl,
			RedirectFailUrl = redirectUrl,
			Metadata = request.Metadata
		};

		logger.LogInformation("Creating Plisio invoice for user {EntityId}, amount: {Amount} {Currency}", 
			user.EntityId, request.Amount, request.Currency);

		var response = await plisioService.CreateInvoiceAsync(invoiceRequest);

		if (!response.IsSuccess)
		{
			logger.LogWarning("Failed to create Plisio invoice: {Message}", response.ErrorMessage);
			return BadRequest(response.ErrorMessage ?? "Failed to create payment");
		}

		return Ok(new PlisioPaymentResponse
		{
			InvoiceUrl = response.InvoiceUrl ?? string.Empty,
			TransactionId = response.TxnId ?? string.Empty
		});
	}

	// Keep GET endpoint for backwards compatibility with existing frontend
	[Authorize]
	[HttpGet("Plisio")]
	public async Task<IActionResult> StartPlisioCryptoPaymentGet(
		[FromQuery(Name = "a")] decimal amount, 
		[FromQuery(Name = "c")] string? currency,
		[FromQuery(Name = "t")] PaymentWindowType paymentWindowType = PaymentWindowType.Deposit,
		[FromQuery(Name = "bonus")] string? bonus = null)
	{
		var metadata = !string.IsNullOrEmpty(bonus) 
			? new Dictionary<string, string> { ["bonus"] = bonus } 
			: null;
		
		var request = new PlisioPaymentRequest
		{
			Amount = amount,
			Currency = currency,
			PaymentWindowType = paymentWindowType,
			Metadata = metadata
		};

		var result = await StartPlisioCryptoPayment(request);
		
		if (result is OkObjectResult okResult && okResult.Value is PlisioPaymentResponse response)
		{
			return Ok(response.InvoiceUrl);
		}

		return result;
	}

	#endregion

	[HttpGet("ExchangeRate")]
	public IActionResult GetExchangeRate([FromQuery] string iso)
	{
		var currencyResponseObj = CurrencyHelper.GetCurrency();

		WebStorageUtility.SetString(WebStorageUtility.CurrencyValueName,
				JsonConvert.SerializeObject(currencyResponseObj),
				WebStorageUtility.GetUserDateTime() + TimeSpan.FromDays(1));

		return Ok(currencyResponseObj);
	}

	public class BalanceRecalculationResponse : IdentityHelper.GeneralApiResponse
	{
		[JsonProperty("Account Balance USD")] public decimal AccountBalanceUsd { get; set; }

		[JsonProperty("Account Balance Local")]
		public decimal AccountBalanceLocal { get; set; }

		public decimal AffiliateWithdrawAllowedSum { get; set; }
		public decimal WithdrawAllowedSum { get; set; }
	}

	[HttpPost("Withdraw")]
	public IActionResult Withdraw([FromBody] WithdrawModel model)
	{
		var isSuccess = PaymentHelper.Withdraw(model, out var response);

		var page = WebStorageUtility.RewriteUrlWithUserIso(
				$"/successfull?d={CryptoUtility.EncryptString($"{model.Amount:0.00}:;:{HttpContext.Request.Headers["currencyIso"]}:;:1")}");

		HttpContext.Response.Headers.Add("redirect", page);

		switch (response.ResultCode)
		{
			case -9:
				{
					WebStorageUtility.TryGetString(WebStorageUtility.LangIso, out var value);
					value = value is "en" ? "" : $"/{value}";

					response.ResultMessage = umbracoHelper
							.GetDictionaryValue("You should confirm you passport to withdraw funds").Replace("{value}", value);
					break;
				}
			case -26:
				{
					WebStorageUtility.TryGetString(WebStorageUtility.LangIso, out var value);
					value = value is "en" ? "" : $"/{value}";

					response.ResultMessage = umbracoHelper
							.GetDictionaryValue(
									"One of your cards is not verified. You should confirm your card to withdraw funds")
							.Replace("{value}", value);
					break;
				}
			default: break;
		}

		if (isSuccess)
		{
			// var user = WebStorageUtility.GetSignedUser();
			// var totalWinningUsd = WebStorageUtility.GetEntityField(user.EntityId, "customfield57");
			// var totalWithdrawAmountUsd = WebStorageUtility.GetEntityField(user.EntityId, "customfield55");
			// user.TotalWinningsUsd = decimal.Parse((string)totalWinningUsd);
			// user.TotalWithdrawAmountUsd = decimal.Parse((string)totalWithdrawAmountUsd);

			//WebStorageUtility.SignIn(HttpContext, user);
			return Ok(response.ResultMessage);
		}
		else
			return BadRequest(response.ResultMessage);
	}

	[HttpPost("Withdraw/Create")]
	public IActionResult WithdrawCreate([FromBody] WithdrawApiModel model)
	{
		var isSuccess = PaymentHelper.CreateWithdrawMethod(model, out var response);

		if (!isSuccess && response.ResultCode == -827485)
		{
			model.CurrencyIso = "USD";
			isSuccess = PaymentHelper.CreateWithdrawMethod(model, out response);
		}

		return isSuccess ? Ok(response.ResultMessage) : BadRequest(response.ResultMessage);
	}

	[HttpGet("Withdraw/Delete/{withdrawId:int}")]
	public IActionResult WithdrawDelete([FromRoute] int withdrawId)
	{
		var isSuccess = PaymentHelper.DeleteWithdrawMethod(withdrawId, out var response);

		return isSuccess ? Ok(response.ResultMessage) : BadRequest(response.ResultMessage);
	}
}