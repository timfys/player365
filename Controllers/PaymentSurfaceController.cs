using GoldCasino.ApiModule.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartWinners.Helpers;
using SmartWinners.PaymentSystem.StartAJob;
using SmartWinners.Services.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SmartWinners.Controllers;
[Microsoft.AspNetCore.Components.Route("")]
public class PaymentSurfaceController(IUmbracoContextAccessor umbracoContextAccessor,
				IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches,
				IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
				PaymentSuccessOrchestrator paymentSuccessOrchestrator,
				ILogger<PaymentSurfaceController> logger) : SurfaceController(umbracoContextAccessor,
		databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
	[HttpGet("{langIso}/withdraw/{pageIndex:int}")]
	[HttpGet("withdraw/{pageIndex:int}")]
	[HttpGet("{langIso}/withdraw")]
	[HttpGet("withdraw")]
	public IActionResult Withdraw([FromRoute] string? langIso, [FromQuery] int? t)
	{
		return View("/Views/Withdraw.cshtml");
	}

	[HttpGet("{langIso}/withdraw-balance/{id:int}")]
	[HttpGet("withdraw-balance/{id:int}")]
	public IActionResult WithdrawBalance([FromRoute] int id, [FromRoute] string? langIso)
	{
		if (id is 0)
			return NotFound();

		ViewBag.Id = id;
		return View("/Views/WithdrawBalance.cshtml");
	}

	[HttpGet("{langIso}/payment-success/")]
	[HttpGet("payment-success/")]
	public IActionResult WithdrawBalance([FromQuery(Name = "p")] string paymentObjEncoded, [FromRoute] string? langIso,
			[FromQuery(Name = "pt")] int paymentType)
	{
		var user = WebStorageUtility.GetSignedUser();

		if (user is null)
			return NotFound();

		ViewBag.PaymentType = paymentType;

		return View("/Views/PaymentSucess.cshtml");
	}

	[HttpGet("{langIso}/successfull")]
	[HttpGet("successfull")]
	public async Task<IActionResult> WithdrawBalance(
		[FromQuery(Name = "d")] string? dataStrEnc, 
		[FromQuery(Name = "dt")] string? dataStr, 
		[FromQuery(Name = "bp")] string? bonusPayload,
		[FromRoute] string? langIso, 
		[FromRoute] string? tId)
	{
		if (dataStrEnc is null && dataStr is null && bonusPayload is null)
			return NotFound();
		var data = Array.Empty<string>();

		if (!string.IsNullOrEmpty(dataStr))
		{
			data = dataStr.Split("---");
			var list = new List<string>();
			list.AddRange(data);
			list.Add(CurrencyHelper.GetCurrency().Symbol);
			data = [.. list];
		}
		else
		{
			data = CryptoUtility.DecryptString(dataStrEnc).Split(":;:");
		}

		var amount = data[0];

		ViewBag.Amount = amount;
		ViewBag.Currency = (await CurrencyHelper.GetCurrencies()).First(x => x.CurrencyIso.ToLower().Equals(data[1].ToLower()));
		ViewBag.Type = int.Parse(data[2]);

		if ((int)ViewBag.Type == 2)
		{
			ViewBag.Symbol = data[3];
		}

		// Run payment success handlers (bonuses) using encrypted payload
		await RunPaymentSuccessHandlersAsync(bonusPayload, amount, data[1]);

		return View("/Views/Succesfull.cshtml");
	}

	/// <summary>
	/// Runs payment success handlers for Stripe payments redirected back via ou parameter.
	/// This applies bonuses that couldn't be applied via webhook since Stripe doesn't use webhooks here.
	/// The bonusPayload is an encrypted string containing: entityId:;:username:;:password:;:businessId:;:lid:;:bonusType
	/// </summary>
	private async Task RunPaymentSuccessHandlersAsync(string? bonusPayload, string amount, string currency)
	{
		if (string.IsNullOrEmpty(bonusPayload))
			return;

		try
		{
			// Decrypt and parse payload: entityId:;:username:;:password:;:businessId:;:lid:;:bonusType
			var decrypted = CryptoUtility.DecryptString(bonusPayload);
			if (string.IsNullOrEmpty(decrypted))
			{
				logger.LogWarning("Failed to decrypt bonus payload");
				return;
			}

			var parts = decrypted.Split(":;:");
			if (parts.Length < 6)
			{
				logger.LogWarning("Invalid bonus payload format, expected 6 parts but got {Count}", parts.Length);
				return;
			}

			var entityId = int.TryParse(parts[0], out var eid) ? eid : 0;
			var username = parts[1];
			var password = parts[2];
			var businessId = int.TryParse(parts[3], out var bid) ? bid : 0;
			var lid = parts[4];
			var bonusType = parts[5];

			if (entityId == 0)
			{
				logger.LogWarning("Invalid entityId in bonus payload");
				return;
			}

			if (string.IsNullOrEmpty(bonusType))
			{
				logger.LogDebug("No bonus type in payload, skipping handlers");
				return;
			}

			var metadata = new Dictionary<string, string>
			{
				["bonus"] = bonusType,
				["lid"] = lid,
				["businessId"] = businessId.ToString()
			};

			var context = new PaymentSuccessContext
			{
				EntityId = entityId,
				RecordId = 0, // Not available from redirect flow
				PaymentType = PaymentWindowType.Deposit,
				TransactionId = $"stripe_{entityId}_{DateTime.UtcNow.Ticks}",
				Amount = decimal.TryParse(amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var amt) ? amt : 0,
				Currency = currency,
				Metadata = metadata
			};

			logger.LogInformation("Running payment success handlers for EntityId {EntityId}, Bonus={Bonus}, Amount={Amount}",
				entityId, bonusType, amount);

			await paymentSuccessOrchestrator.ExecuteAsync(context);

			logger.LogInformation("Payment success handlers completed for EntityId {EntityId}", entityId);
		}
		catch (Exception ex)
		{
			// Log but don't fail - user should still see success page
			logger.LogError(ex, "Error running payment success handlers from payload");
		}
	}

	[HttpGet("Purchase-Coins")]
	[HttpGet("{langIso}/Purchase-Coins")]
	[HttpGet("Purchase-Coins/{trId}")]
	[HttpGet("{langIso}/Purchase-Coins/{trId}")]
	public IActionResult Payment([FromRoute(Name = "langIso")] string? langIso,
				[FromQuery(Name = "payment_intent")] string? sessionId, [FromRoute(Name = "trId")] string? plisioTransactionId, [FromQuery(Name = "e")] string? exception)
	{
		var isAuzorized = HttpContext.User.Identity.IsAuthenticated;

		if (!isAuzorized)
			return Redirect($"{langIso}/sign-in?r={langIso}/purchase-coins");

		var amount = HttpContext.Request.Query["a"].FirstOrDefault();
		if (string.IsNullOrEmpty(amount))
			amount = "20";

		var userCurrency = WebStorageUtility.GetUserCurrencyDetails();
		var currencyCode = userCurrency.CurrencyIso;
		var currencyCodeToSave = userCurrency.CurrencyIso;
		if (WebStorageUtility.TryGetString(WebStorageUtility.LastUsedCurrencyIso, out var currency))
		{
			currencyCode = currency;
			currencyCodeToSave = currency;
		}

		var amountD = decimal.Parse(amount, WebStorageUtility.GetUserCultureInfo());

		ViewBag.Amount = CheckIfMinimalAmount(amountD, HttpContext.Request.Query.ContainsKey("IsL"), userCurrency.ExchangeRate);

		var transactionType = Request.Query["tT"];

		WebStorageUtility.SetString(WebStorageUtility.LastUsedCurrencyIso, currencyCodeToSave);
		ViewBag.TransactionId = string.IsNullOrEmpty(plisioTransactionId)
				? string.IsNullOrEmpty(sessionId) ? "" : sessionId
				: plisioTransactionId;
		ViewBag.TransactionType = !string.IsNullOrEmpty(sessionId) ? "5" : transactionType.FirstOrDefault();
		ViewBag.LastUsedCurrencyCode = currencyCodeToSave;
		ViewBag.DomainType = DomainsHelper.GetDomainNameType();
		ViewBag.Exception = exception;

		return View(DomainsHelper.GetDomainNameType() is DomainType.PlayerClub or DomainType.PlayerClubTest ? "/Views/ToStripeDepositPage.cshtml" : "/Views/ToStripeDepositPage.cshtml");
	}
	
	[NonAction]
	public decimal CheckIfMinimalAmount(decimal amount, bool isLocal, decimal userCurrencyExchangeRate)
	{
		if (!isLocal && amount < 1)
			return 5;
		if (isLocal && amount < 1 * userCurrencyExchangeRate)
			return 1 * userCurrencyExchangeRate;
		return amount;
	}
}