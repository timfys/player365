using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PhoneNumbers;
using SmartWinners.Helpers;
using System.Web;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Controllers;

public class NormalVerifyPhoneController(UmbracoHelper umbracoHelper) : Controller
{
  private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

  [HttpGet]
	[Route("verify-phone")]
	[Route("{langIso?}/verify-phone")]
	public IActionResult Render([FromRoute] string? langIso)
	{
		var isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
		var user =  HttpContext.User.ToUserApiAccess();	

		langIso = string.IsNullOrEmpty(langIso) ? "" : $"/{langIso}";

		if (HttpContext.Request.Query.TryGetValue("user", out var userQuery))
		{
			var userPhone = CryptoUtility.DecryptObject<string>(HttpUtility.UrlDecode(userQuery));
			var phoneNumber = PhoneHelper.Parse(userPhone);
			ViewBag.PhonePrefix = phoneNumber?.CountryCode;
			ViewBag.Phone = phoneNumber?.NationalNumber.ToString();
			ViewBag.PhoneCountry = _phoneUtil.GetRegionCodeForNumber(phoneNumber);
		}
		else 
			return Redirect(UmbracoUtility.GetReturnUrl(HttpContext));
		
		ViewBag.IsFourDigitCode = false;

		return View("/Views/signupVerfication1.cshtml");
	}

	[HttpPost("Process/SendAgainWhatsapp")]
	public IActionResult SendAgainWhatsapp([FromQuery(Name = "p")] string phone, [FromQuery(Name = "i")] string iso, [FromQuery(Name = "pr")] string prefix)
	{
		var config = EnvironmentHelper.WhatsAppConfiguration;
		var client = config.InitClient();

		var apiResponse = client.SendVerifyingMessageAsync(ol_EntityId: config.ol_EntityId, ol_Username: config.ol_UserName, ol_Password: config.ol_Password, Mobile: prefix + phone, CountryISO: iso).Result;

		var response = JsonConvert.DeserializeObject<IdentityHelper.GeneralApiResponse>(apiResponse.@return);

		if (response.IsSuccess())
		{
			response.ResultMessage = umbracoHelper.GetDictionaryValueOrDefault("We have sent an WhatsApp to", "We have sent an WhatsApp to").Replace("{0}", $"{prefix}{phone}");
			return Ok(response);
		}
		else
		{
			return BadRequest(response);
		}
	}


	[HttpPost("Process/SendAgain4Digit")]
	public IActionResult SendAgain4Digit([FromQuery(Name = "eId")] int entityId)
	{
		return IdentityHelper.TrySendAgain4Digit(entityId, out var message) ? Ok(umbracoHelper.GetDictionaryValueOrDefault("We have sent an SMS to", "We have sent an SMS to")) : BadRequest(message);
	}

	[HttpPost("Process/Verify4Digit")]
	public IActionResult Verify4Digit([FromQuery(Name = "eId")] int entityId, [FromQuery(Name = "c")] string code)
	{
		if (IdentityHelper.TryVerify4Digit(entityId, code, out var message))
		{
			//var user = WebStorageUtility.GetSignedUser();
			// user.MobileVerified = true;

			//WebStorageUtility.SignIn(HttpContext, user);
			HttpContext.Response.Headers.Add("Redirect", HttpUtility.UrlEncode(UmbracoUtility.GetReturnUrl(HttpContext)));
			return Ok(message);
		}

		return BadRequest(message);
	}
}