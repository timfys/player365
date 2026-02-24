using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SmartWinners.Controllers;

public class HistoryPlayedController(IUmbracoContextAccessor umbracoContextAccessor,
		IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches,
		IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider) : SurfaceController(umbracoContextAccessor,
		databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{


	[Route("/played-history")]
	[Route("{langIso}/played-history")]
	public IActionResult HistoryPlayedPage([FromRoute] string? langIso, [FromQuery] int? page)
	{
		try
		{
			var isAuzorized = HttpContext.User.Identity?.IsAuthenticated ?? false;
			if (!isAuzorized)
				return Redirect($"{langIso}/sign-in?r={langIso}/played-history");

			ViewBag.PageIndex = page ?? 1;
			return View("/Views/HistoryPlayedPage.cshtml");
		}
		catch
		{
			return BadRequest();
		}
	}
}