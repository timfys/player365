using Microsoft.AspNetCore.Mvc;
using SmartWinners.Helpers;
using SmartWinners.Services;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Controllers;

[Route("")]
public class NormalSignInController(UmbracoHelper umbracoHelper, IIpInfoCountryResolver ipCountryResolver) : Controller
{
	[HttpGet]
	[Route("sign-in")]
	[Route("{langIso}/sign-in")]
	public async Task<IActionResult> Render([FromRoute] string? langIso, [FromQuery] int? page, [FromQuery] bool? min)
	{
		langIso = string.IsNullOrEmpty(langIso) ? "" : $"/{langIso}";

		if (HttpContext.User.Identity?.IsAuthenticated ?? false)
		{
			var returnUrl = UmbracoUtility.GetReturnUrl(HttpContext);
			return Redirect(returnUrl);
		}

		var ipAddress = IdentityHelper.GetUserIp(HttpContext);
		var iso = IdentityHelper.GetUserIsoFromCloudFlare(HttpContext)?.ToLowerInvariant() ?? 
							await ipCountryResolver.GetCountryIsoAsync(ipAddress) ?? 
							"us";
		ViewBag.Iso = iso;

		if (min is false || min is null)
			if (page is not null and 1)
			{
				ViewBag.PageIndex = "1";
				ViewBag.Min = min ?? false;
				return View("/Views/SignInPage.cshtml");
			}

		ViewBag.PageIndex = "0";
		ViewBag.Min = min ?? false;

		return View("/Views/SignInPage.cshtml");
	}
}