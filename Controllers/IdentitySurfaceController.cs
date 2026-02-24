using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Enums;
using Microsoft.AspNetCore.Mvc;
using SmartWinners.Helpers;
using System.Linq;
using System.Threading.Tasks;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SmartWinners.Controllers;

public class IdentitySurfaceController(IUmbracoContextAccessor umbracoContextAccessor,
		IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IBusinessApiService businessApiService,
		IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : SurfaceController(umbracoContextAccessor,
		databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
	[Route("{langIso}/identity")]
	[Route("identity")]
	[Route("{langIso}/identity/{pageIndex:int}")]
	[Route("identity/{pageIndex:int}")]
	[Route("{langIso}/Card-Verification-Charge/{cardLastNums}")]
	[Route("Card-Verification-Charge/{cardLastNums}")]
	[Route("{langIso}/Card-Verification-Confirm/{cardLastNums}")]
	[Route("Card-Verification-Confirm/{cardLastNums}")]
	public async Task<IActionResult> IdentityVerifyPage([FromRoute] int pageIndex, [FromRoute] string? cardLastNums)
	{
		pageIndex = pageIndex is 0 ? 1 : pageIndex;

		var isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
		var identityUser = HttpContext.User.ToUserApiAccess();

		var result = await businessApiService.EntityFindAsync(new EntityFind()
		{
			Fields = FieldHelper<UserVerificationStateDto>.Fields,
			Filter = new() { { "entityId", $"{identityUser?.EntityId}" } }
		}, identityUser);

		var entity = result.Value?.Entities.FirstOrDefault();
		var userPassportState = EntityMapper.MapTo<UserVerificationStateDto>(entity).VerificationState;

		if (pageIndex is 0 or 1 && userPassportState is IdDocVerificationState.Approved
						or IdDocVerificationState.Declined or IdDocVerificationState.PendingVerification)
		{
			ViewBag.UserVerificationDoc = userPassportState;
		}

		var requestPath = HttpContext.Request.Path.Value.ToLower();

		if (requestPath.Contains("card-verification"))
		{
			if (!isAuthenticated)
			{
				return NotFound();
			}

			var cards = await PaymentHelper.GetUserPaymentCards(identityUser.EntityId);

			var card = cards?.FirstOrDefault(x => x.PayerNumber.EndsWith(cardLastNums));

			if (card is null)
			{
				return NotFound();
			}

			ViewBag.Card = card;

			pageIndex = requestPath.Contains("card-verification-charge") ? 2 : 3;
		}

		ViewBag.PageIndex = pageIndex;
		return View("~/Views/IdentityVerifyPage.cshtml");
	}
}