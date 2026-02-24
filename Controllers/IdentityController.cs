using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWinners.Helpers;
using System;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Controllers;

[Authorize]
[Route("Identity")]
public class IdentityController(UmbracoHelper umbracoHelper, IBusinessApiService businessApiService) : Controller
{
	[HttpPost("UploadIdDocument")]
	[RequestSizeLimit(100 * 1024 * 1024)] //100 megabytes
	public async Task<IActionResult> UploadIdDocument([FromBody] FileMetaData fileMetaData,
			[FromHeader(Name = "UserPassportState")] string cardState)
	{
		var user = HttpContext.User.ToUserApiAccess();
		if (Enum.TryParse<IdDocVerificationState>(cardState, out var state))
		{
			//var response = await IdentityHelper.UploadFile(fileMetaData);
			var response = await businessApiService.EntityFilesUpdateAsync(new()
			{
				EntityId = user.EntityId,
				Filename = fileMetaData.FileName,
				Width = fileMetaData.Width,
				Height = fileMetaData.Height,
				FileId = 0,
				FileData = Convert.FromBase64String(fileMetaData.Base64FileString)
			});
			if (response.IsSuccess && fileMetaData.FileName.Equals("passport"))
			{
				IdentityHelper.UpdateEntity(new() { { "CustomField109", "1" } }, user.EntityId,
							out var updateResponse);

				return response.IsSuccess ? Ok(response.Value?.ResultMessage) : BadRequest(response.Value?.ResultMessage);
			}
		}

		return BadRequest("Incorrect identification document state");
	}

	[HttpPost("UploadFile")]
	[RequestSizeLimit(10 * 1024 * 1024)] //10 megabytes
	public async Task<IActionResult> UploadFile([FromBody] FileMetaData fileMetaData)
	{
		var user = HttpContext.User.ToUserApiAccess();

		var response = await businessApiService.EntityFilesUpdateAsync(new()
		{
			EntityId = user.EntityId,
			Filename = fileMetaData.FileName,
			Width = fileMetaData.Width,
			Height = fileMetaData.Height,
			FileId = 0,
			FileData = Convert.FromBase64String(fileMetaData.Base64FileString)
		});

		return response.IsSuccess ? Ok(response.Value?.ResultMessage) : BadRequest(response.Value?.ResultMessage);
	}

	[HttpPost("VerifyCard")]
	public async Task<IActionResult> VerifyCard([FromBody] UserVerifyCardModel? cardModel,
			[FromHeader(Name = "ToVerify")] string toVerify)
	{
		cardModel.ExpireDate = CryptoUtility.DecryptString(cardModel.ExpireDate);
		cardModel.CardNumber = CryptoUtility.DecryptString(cardModel.CardNumber);
		cardModel.PayerName = CryptoUtility.DecryptString(cardModel.PayerName);

		var response = await IdentityHelper.VerifyCard(HttpContext, cardModel, toVerify is "1");

		switch (response.ResultCode)
		{
			case -102:
				{
					response.ResultMessage = umbracoHelper.GetDictionaryValueOrDefault("Verification amount is not match", "Verification amount is not match");
					break;
				}
			case -101:
				{
					response.ResultMessage = umbracoHelper.GetDictionaryValueOrDefault("Number of attempts exceeded", "Number of attempts exceeded");
					break;
				}
		}

		return response.IsSuccess() ? Ok(response.ResultMessage) : BadRequest(response.ResultMessage);

	}
}