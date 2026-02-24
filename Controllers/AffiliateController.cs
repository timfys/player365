using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SmartWinners.Controllers;

[Route("")]
public class AffiliateController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
		ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger,
		IPublishedUrlProvider publishedUrlProvider) : 
		SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches,
    profilingLogger, publishedUrlProvider)
{

	/*[Route("/landing")]
	[Route("/{langIso}/landing")]
	public IActionResult Affiliate([FromRoute(Name = "langIso")] string langIso,
			[FromQuery(Name = "aID")] int affiliateId, [FromQuery(Name = "ltrID")] int lotteryId)
	{
			_umbracoContextAccessor.TryGetUmbracoContext(out var context);

			var content = context.PublishedRequest.PublishedContent;

			if (LotteryHelper.GetLottery(lotteryId, out var lottery))
			{

					var fallBack = new PublishedValueFallback(Services, _variationContextAccessor);

					var model = new SignInPage(content, fallBack);

					ViewBag.Lottery = lottery;
					ViewBag.AffiliateId = affiliateId;
					return View("/Views/LandingPage.cshtml", model);
			}

			return NotFound();

	}*/
}