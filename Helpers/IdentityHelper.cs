using BusinessApi;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Integrations.SmartWinners;
using GoldCasino.ApiModule.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SmartWinners.PaymentSystem.StartAJob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Entity_VerifyContactInfoRequest = BusinessApi.Entity_VerifyContactInfoRequest;

namespace SmartWinners.Helpers;

public class IdentityHelper
{
	public static string? GetUserIsoFromCloudFlare(HttpContext httpContext)
	{
		var host = httpContext?.Request.Host.Value;
		if (host?.Contains("player", StringComparison.OrdinalIgnoreCase) ?? false)
			return httpContext?.Request.Headers["CF-IPCountry"];

		return "IL";
	}

	public static async Task<List<UserVerifyCardModelRender>> GetUserCardsToVerify(UserApiAccess apiAccessData)
	{
		var client = EnvironmentHelper.SmartWinnersApiConfiguration.InitClient();

		var apiRequest = new Entity_Payments_VerificationRequest
		{
			ol_EntityID = apiAccessData.EntityId,
			ol_Username = apiAccessData.Username,
			ol_Password = apiAccessData.Password,
			BusinessId = 1,
			VerificationType = 1
		};

		var apiResponse = await client.Entity_Payments_VerificationAsync(apiRequest);

		try
		{
			return JsonConvert.DeserializeObject<List<UserVerifyCardModelRender>>(apiResponse.@return);
		}
		catch
		{
			return [];
		}
	}

	public static bool UpdateEntity(Dictionary<string, string> updateFields, int entityId,
			out GeneralApiResponse apiResponse)
	{
		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var config = EnvironmentHelper.BusinessApiConfiguration;

		var request = new Entity_UpdateRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			EntityId = entityId,
			NamesArray = [.. updateFields.Keys],
			ValuesArray = [.. updateFields.Values]
		};

		var response = client.Entity_Update(request);

		apiResponse = JsonConvert.DeserializeObject<GeneralApiResponse>(response.@return);

		if (apiResponse is null)
			return false;

		return apiResponse.IsSuccess();
	}

		public static bool TrySendAgain4Digit(int entityId, out string error)
	{
		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var config = EnvironmentHelper.BusinessApiConfiguration;

		var request = new Entity_VerifyContactInfoRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			businessId = config.BusinessId,
			entityID = entityId,
			VerifyType = 0,
			VerificationCode = null
		};


		var responseJson = client.Entity_VerifyContactInfo(request).@return;
		var result = JsonConvert.DeserializeObject<EntityVerifyResult>(responseJson);
		error = result.ResultMessage;

		return result.IsSuccess();
	}

	public static bool TryVerify4Digit(int entityId, string code, out string error)
	{
		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var config = EnvironmentHelper.BusinessApiConfiguration;

		var request = new Entity_VerifyContactInfoRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			businessId = config.BusinessId,
			entityID = entityId,
			VerifyType = 0,
			VerificationCode = code
		};


		var responseJson = client.Entity_VerifyContactInfo(request).@return;
		var result = JsonConvert.DeserializeObject<EntityVerifyResult>(responseJson);
		error = result.ResultMessage;

		return result.IsSuccess();
	}

	public class GeneralApiResponse
	{
		public int ResultCode { get; set; }
		public string ResultMessage { get; set; }
		public bool IsSuccess() => ResultCode >= 0;

		public bool CheckIfUserExists()
		{
			if (ResultCode == -1)
			{
				var context = EnvironmentHelper.HttpContextAccessor.HttpContext;
				context.Response.Redirect("/sign-out");
				WebStorageUtility.SignOut(context);
				return false;
			}

			return true;
		}
	}

public class EntityVerifyResult
	{
		public const int CodeAlreadySent = -17;

		public int ResultCode { get; set; }
		public string ResultMessage { get; set; }
		public string PhoneNumber { get; set; }

		public string Lid { get; set; }

		public bool IsSuccess() => ResultCode >= 0;
	}

	public static void SetReturnUrl(HttpContext context)
	{
		try
		{
			string returnUrl;

			if (context.Request.Query["ReturnUrl"].FirstOrDefault() is not null)
			{
				returnUrl = context.Request.Query["ReturnUrl"].FirstOrDefault();
			}
			else
			{
				returnUrl = context.Request.Headers["Referer"].FirstOrDefault();
			}

			returnUrl = returnUrl is null ? "./" : returnUrl;

			if (returnUrl.ToLower().Contains("sign-up") || returnUrl.ToLower().Contains("sign-in") ||
					returnUrl.ToLower().Contains("verify-phone"))
			{
				returnUrl = "./";
			}

			WebStorageUtility.SetString("ReturnUrl", returnUrl);
		}
		catch (Exception e)
		{
			WebStorageUtility.SetString("ReturnUrl", "./");
		}
	}

	public static async Task<GeneralApiResponse> VerifyCard(HttpContext httpContext, UserVerifyCardModel cardModel, bool toVerify)
	{
		var isAuzorized = httpContext.User.Identity?.IsAuthenticated;
		var user = httpContext.User.ToUserApiAccess();

		if (user is null)
		{
			return new GeneralApiResponse
			{
				ResultMessage = "User is not logged in",
				ResultCode = -1
			};
		}

		var client = EnvironmentHelper.SmartWinnersApiConfiguration.InitClient();

		var apiRequest = new Entity_Payments_VerificationRequest
		{
			ol_EntityID = user.EntityId,
			ol_Username = user.Username,
			ol_Password = user.Password,
			BusinessId = 1,
			VerificationType = toVerify ? 3 : 2,
			CardNumber = cardModel.CardNumber,
			ExpirationDate = cardModel.ExpireDate
		};
		if (toVerify)
		{
			apiRequest.VerificationAmount = cardModel.Amount;
		}
		else
		{
			apiRequest.CVV = cardModel.Cvv;
		}

		var apiResponse = await client.Entity_Payments_VerificationAsync(apiRequest);

		var resp = JsonConvert.DeserializeObject<GeneralApiResponse>(apiResponse.@return);

		if (apiRequest.VerificationType == 3 && resp.IsSuccess())
			await PaymentHelper.GetUserBalance(user, false);

		if (!resp.IsSuccess())
		{
			PaymentHelper.LogPayments(PaymentWindowType.CardVerification, toVerify ? $"{cardModel.Amount}" : "0", "",
					resp.ResultMessage, JsonConvert.SerializeObject(apiRequest), user.EntityId);
		}

		return resp;
	}

	public static string? GetUserIp(HttpContext? context = null)
	{
		var httpContext = context ?? EnvironmentHelper.HttpContextAccessor.HttpContext;
		if (httpContext == null)
			return null;

		// 1. Cloudflare header
		var cfIp = httpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
		if (!string.IsNullOrWhiteSpace(cfIp))
			return cfIp;

		// 2. Standard reverse proxy header
		var xff = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrWhiteSpace(xff))
		{
			// may contain: "client, proxy1, proxy2"
			var firstIp = xff.Split(',').Select(x => x.Trim()).FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(firstIp))
				return firstIp;
		}

		// 3. Fallback to connection address
		return httpContext.Connection.RemoteIpAddress?.ToString();
	}

	public static async Task UpdateUserLangIsoAsync(string langIso, int? entityId = null)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Entity_UpdateRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			EntityId = entityId ?? WebStorageUtility.GetSignedUser()!.EntityId,
			NamesArray = ["CustomField67"],
			ValuesArray = [langIso]
		};

		var resp = await client.Entity_UpdateAsync(apiRequest);
	}

	public static async Task<int> CheckForNotificationEnabled(int entityId)
	{
		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

		var apiRequest = new Entity_FindRequest()
		{
			ol_EntityId = config.ol_EntityId,
			ol_UserName = config.ol_UserName,
			ol_Password = config.ol_Password,
			BusinessId = config.BusinessId,
			FilterFields = ["EntityId"],
			FilterValues = [$"{entityId}"],
			Fields = ["CustomField146"]
		};

		var apiResponse = await client.Entity_FindAsync(apiRequest);

		var response = JsonConvert.DeserializeObject<List<CustomFieldsResponse>>(apiResponse.@return);

		return int.TryParse(response?.FirstOrDefault()?.TelegramNotificationsIsEnabled, out var resp) ? resp : 0;
	}
	public static FileModel? GetFile(string name)
	{
		var user = WebStorageUtility.GetSignedUser();

		if (user is null)
			return null;

		var config = EnvironmentHelper.BusinessApiConfiguration;

		var client = config.InitClient();

		var apiRequest = new Entity_Files_GetRequest
		{
			ol_EntityID = config.ol_EntityId,
			ol_Username = config.ol_UserName,
			ol_Password = config.ol_Password,
			BusinessId = config.BusinessId,
			Fields = new[] { "Filename", "FileData" },
			FilterFields = new[] { "Filename", "EntityId" },
			FilterValues = new[] { $"{name}", $"{user.EntityId}" }
		};

		var apiResponse = client.Entity_Files_Get(apiRequest);

		var response = JsonConvert.DeserializeObject<List<FileModel>>(apiResponse.@return)?.FirstOrDefault();

		if (response is null)
			return null;

		response.Base64ImgString = Convert.ToBase64String(response.ByteArrayImg);

		return response;
	}
}


public class CustomFieldsResponse
{
	[JsonProperty("CustomField146")]
	public string TelegramNotificationsIsEnabled { get; set; }
}

public class FileModel
{
	[JsonProperty("fileid")] public int FileId { get; set; }

	[JsonProperty("FileData")] /* [JsonConverter(typeof(JsonHelper.ByteArrayConverter))]*/
	public byte[] ByteArrayImg { get; set; }

	public string Base64ImgString { get; set; }

	[JsonProperty("Filename")] public string FileName { get; set; }
}

public class UserVerifyCardModel
{
	public string CardNumber { get; set; }
	public string? Cvv { get; set; }
	public string? AmountStr { get; set; }
	public double Amount { get; set; }
	public string PayerName { get; set; }
	public string ExpireDate { get; set; }
}

public class UserVerifyCardModelRender
{
	[JsonProperty("customfield112")] public DateTime? VerificationRequestDate { get; set; }

	[JsonProperty("customfield119")] public string CardLastNumbers { get; set; }

	public float? VerificationAmount { get; set; }

	[JsonProperty("customfield114")] public CardVerificationState CardVerificationState { get; set; }

	public string GetCardPaymentSystemName()
	{
		switch (CardLastNumbers)
		{
			case var tempStr when Regex.IsMatch(tempStr, "^4[0-9]{12}(?:[0-9]{3})?$"):
				{
					return @"Visa";
				}
			case var tempStr when Regex.IsMatch(tempStr, "^5[1-5][0-9]{14}$"):
				{
					return @"MasterCard";
				}
			case var tempStr
					when Regex.IsMatch(tempStr, "^(5018|5020|5038|5893|6304|6759|6761|6762|6763)[0-9]{8,15}$"):
				{
					return @"Maestro";
				}
			default:
				{
					return @"";
				}
		}
	}

	public string GetCardImgUrl()
	{
		switch (CardLastNumbers)
		{
			case var tempStr when Regex.IsMatch(tempStr, "^4[0-9]{12}(?:[0-9]{3})?$"):
				{
					return @"/images/homepage/Visa-dark.svg";
				}
			case var tempStr when Regex.IsMatch(tempStr, "^5[1-5][0-9]{14}$"):
				{
					return @"/images/homepage/MasterCard-dark.svg";
				}
			case var tempStr
					when Regex.IsMatch(tempStr, "^(5018|5020|5038|5893|6304|6759|6761|6762|6763)[0-9]{8,15}$"):
				{
					return @"/images/contacts/Maestro-dark.svg";
				}
			default:
				{
					return @"/images/image5.svg";
				}
		}
	}
}

public enum CardVerificationState
{
	NotVerified = 0,
	Verified = 1,
	PendingVerification = 2
}

public class ApiAccessData
{
	public int EntityId { get; set; }
	public string UserName { get; set; }
	public string Password { get; set; }
}

	// public static User FindUser(Dictionary<string, string> filter, BusinessApiConfiguration config = null)
	// {
	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var propertiesName = new List<string>();

	// 	foreach (var propertyInfo in typeof(EntityPublic).GetProperties())
	// 	{
	// 		if (!propertyInfo.Name.Equals("lid") &&
	// 				!propertyInfo.Name.ToLower().Equals("resultmessage") &&
	// 				!propertyInfo.Name.ToLower().Equals("resultcode") /*&&
	//               !propertyInfo.Name.ToLower().Equals("customfield98")*/)
	// 		{
	// 			propertiesName.Add(propertyInfo.Name);
	// 		}
	// 	}

	// 	config = EnvironmentHelper.BusinessApiConfiguration;


	// 	//propertiesName.Remove("lid");

	// 	var request = new Entity_FindRequest
	// 	{
	// 		ol_EntityId = config.ol_EntityId,
	// 		ol_UserName = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		BusinessId = config.BusinessId,
	// 		FilterFields = [.. filter.Keys],
	// 		FilterValues = [.. filter.Values],
	// 		Fields = [.. propertiesName],
	// 		LimitCount = 1
	// 	};


	// 	var response = client.Entity_Find(request);
	// 	try
	// 	{
	// 		var responseObj = JsonConvert.DeserializeObject<List<EntityPublic>>(response.@return)
	// 				.FirstOrDefault();

	// 		if (responseObj is not null && responseObj.IsSuccess())
	// 		{
	// 			var user = MapToUser(responseObj);

	// 			user.NotUsedGifts = GiftHelper.GetNotUsedGifts(user.EntityId, user.UserName, user.Password);
	// 			return user;
	// 		}

	// 		return null;
	// 	}
	// 	catch
	// 	{
	// 		return null;
	// 	}
	// }

	//! USE CustomField68
	// public static GeneralApiResponse SendVerifyEmail()
	// {
	// 	var user = WebStorageUtility.GetSignedUser();
	// 	var config = EnvironmentHelper.BusinessApiConfiguration;
	// 	var businessClient = config.InitClient();

	// 	if (string.IsNullOrEmpty((string)WebStorageUtility.GetEntityField(user.EntityId, "CustomField68")))
	// 	{
	// 		UpdateEntity(new Dictionary<string, string> { { "CustomField68", EnvironmentHelper.HttpContextAccessor.HttpContext.Request.Host.Value } }, user.EntityId, out var resp);
	// 	}

	// 	var request = new Entity_VerifyContactInfoRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		businessId = config.BusinessId,
	// 		entityID = user.EntityId,
	// 		VerifyType = 1,
	// 		VerificationCode = "",
	// 	};


	// 	var apiResponse = businessClient.Entity_VerifyContactInfo(request);

	// 	return JsonConvert.DeserializeObject<GeneralApiResponse>(apiResponse.@return);
	// }

	// public static async Task<GeneralApiResponse> UploadFile(FileMetaData fileMetaData)
	// {
	// 	var config = EnvironmentHelper.BusinessApiConfiguration;

	// 	var user = WebStorageUtility.GetSignedUser();

	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var apiRequest = new Entity_Files_UpdateRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		BusinessId = config.BusinessId,
	// 		NamesArray = [
	// 								"EntityId",
	// 									"Filename",
	// 									"width",
	// 									"Height"],
	// 		ValuesArray = [
	// 								$"{user.EntityId}",
	// 									fileMetaData.FileName,
	// 									$"{fileMetaData.Width}",
	// 									$"{fileMetaData.Height}"],
	// 		FileId = user.VerificationDocFileId,
	// 		FileData = Convert.FromBase64String(fileMetaData.Base64FileString)
	// 	};

	// 	if (fileMetaData.FileName.Equals("profile_img") && user.ProfileImageId != 0)
	// 	{
	// 		apiRequest.FileId = user.ProfileImageId;
	// 	}

	// 	var apiResponse = await client.Entity_Files_UpdateAsync(apiRequest);

	// 	var response = JsonConvert.DeserializeObject<EntityFileResponse>(apiResponse.@return);

	// 	if (response.IsSuccess() && fileMetaData.FileName.Equals("passport"))
	// 	{
	// 		user.VerificationDocFileId = response.FileId;

	// 		UpdateEntity(new Dictionary<string, string> { { "CustomField109", "1" } }, user.EntityId,
	// 				out var updateResponse);
	// 		if (updateResponse.IsSuccess())
	// 		{
	// 			user.IdDocVerificationState = IdDocVerificationState.PendingVerification;
	// 		}

	// 		WebStorageUtility.SignIn(EnvironmentHelper.HttpContextAccessor.HttpContext, user);
	// 	}

	// 	if (fileMetaData.FileName.Equals("profile_img"))
	// 	{
	// 		user.ProfileImageId = response.FileId;
	// 		UpdateEntity(new Dictionary<string, string> { { "profile_imageID", $"{response.FileId}" } }, user.EntityId,
	// 				out var resp);
	// 		WebStorageUtility.SignIn(EnvironmentHelper.HttpContextAccessor.HttpContext, user);
	// 	}

	// 	return response;
	// }

	// public static async Task UpdateUserLoginDataAsync(int entityId, string loginIp, string langIso)
	// {
	// 	var config = EnvironmentHelper.BusinessApiConfiguration;

	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var apiRequest = new Entity_UpdateRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		EntityId = entityId,
	// 		NamesArray = ["CustomField10", "CustomField4", "CustomField67"],
	// 		ValuesArray = [loginIp, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), langIso]
	// 	};

	// 	var apiResponse = await client.Entity_UpdateAsync(apiRequest);
	// }

	// public static int GetUserVerificationDocId(User user = null)
	// {
	// 	var config = EnvironmentHelper.BusinessApiConfiguration;

	// 	if (user is null)
	// 		user = WebStorageUtility.GetSignedUser();

	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var apiRequest = new Entity_Files_GetRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		BusinessId = config.BusinessId,
	// 		Fields = new[] { "Filename" },
	// 		FilterFields = new[] { "EntityId", "Filename" },
	// 		FilterValues = new[] { $"{user.EntityId}", "passport" }
	// 	};

	// 	var apiResponse = client.Entity_Files_Get(apiRequest);

	// 	var response = JsonConvert.DeserializeObject<List<FileModel>>(apiResponse.@return);

	// 	if (response is null || response.Count == 0)
	// 		return 0;

	// 	return response.OrderBy(x => x.FileId).First().FileId;
	// }

	// public static int GetUserProfileImageId(User user = null)
	// {
	// 	var config = EnvironmentHelper.BusinessApiConfiguration;

	// 	if (user is null)
	// 		user = WebStorageUtility.GetSignedUser();

	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var apiRequest = new Entity_Files_GetRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		BusinessId = config.BusinessId,
	// 		Fields = new[] { "Filename" },
	// 		FilterFields = new[] { "EntityId", "Filename" },
	// 		FilterValues = new[] { $"{user.EntityId}", "profile_img" }
	// 	};

	// 	var apiResponse = client.Entity_Files_Get(apiRequest);

	// 	var response = JsonConvert.DeserializeObject<List<FileModel>>(apiResponse.@return);

	// 	if (response is null || response.Count == 0)
	// 		return 0;

	// 	return response.OrderBy(x => x.FileId).First().FileId;
	// }

	// private static User MapToUser(EntityPublic entity)
	// {
	// 	decimal.TryParse(entity.customfield54, NumberStyles.Number, new CultureInfo("en-US"), out var balanceUsd);
	// 	decimal.TryParse(entity.customfield82, NumberStyles.Number, new CultureInfo("en-US"), out var balanceLocal);
	// 	decimal.TryParse(entity.customfield104, NumberStyles.Number, new CultureInfo("en-US"), out var userAffiliateEarnings);
	// 	Enum.TryParse(entity.customfield109, out IdDocVerificationState passportState);

	// 	/*WebStorageUtility.SetString("Lid", entity.lid, WebStorageUtility.LifetimeCookieDate);*/

	// 	var user = new User
	// 	{
	// 		Country = entity.Country,
	// 		Email = entity.Email,
	// 		EntityId = entity.EntityId,
	// 		FirstName = entity.FirstName,
	// 		LastName = entity.LastName,
	// 		Mobile = entity.Mobile,
	// 		MobileVerified = entity.mobile_verified != 0,
	// 		BalanceUSD = balanceUsd,
	// 		BalanceLocal = balanceLocal,
	// 		Password = entity.ol_password,
	// 		UserName = entity.ol_username,
	// 		PhonePrefix = entity.ol_username.Replace(entity.Mobile, ""),
	// 		IdDocVerificationState = passportState,
	// 		UserAffiliateEarnings = userAffiliateEarnings,
	// 		TotalWinningsUsd = entity.customfield57,
	// 		UserAffiliateReferred = entity.customfield131,
	// 		TotalWithdrawAmountUsd = entity.customfield55,
	// 		AffiliateId = entity.affiliateID,
	// 		State = entity.State,
	// 		ZipCode = entity.Zip,
	// 		Address = entity.Address,
	// 		City = entity.City,
	// 		EmailVerified = entity.email_verified,
	// 		LimitedTimeOfferInfo = new GiftHelper.LimitedTimeOfferInfo
	// 		{
	// 			LimitTimeUtc = entity.customfield179,
	// 			PercentageBonusAmount = entity.customfield180 / 100 + 1,
	// 			MinDepositAmount = entity.customfield181,
	// 		},
	// 		SuspendType = entity.customfield183,
	// 		Player1TotalWinnings = entity.customfield185
	// 	};

	// 	user.SetVirtualBalance(entity.customfield178);

	// 	return user;
	// }

	// public class EntityAddResult
	// {
	// 	public const int ResultCodeOK = 0;

	// 	/// <summary>
	// 	/// ResultMessage="Customer exists",  EntityId is returned
	// 	/// </summary>
	// 	public const int ResultCodeCustomerExists = -5674;

	// 	public int? ResultCode { get; set; }
	// 	public string ResultMessage { get; set; }
	// 	public int EntityId { get; set; }
	// 	public int ExecuteTime { get; set; }

	// 	public bool IsCreated => ResultCode == ResultCodeOK;
	// 	public bool IsAlreadyExists => ResultCode == ResultCodeCustomerExists;
	// }



	// private class EntityPublic : GeneralApiResponse
	// {
	// 	public string customfield183 { get; set; }
	// 	public decimal customfield181 { get; set; }
	// 	public decimal customfield180 { get; set; }
	// 	public DateTime? customfield179 { get; set; }
	// 	public int customfield131 { get; set; }

	// 	/*public int customfield53 { get; set; }*/
	// 	public int EntityId { get; set; }
	// 	public string FirstName { get; set; }
	// 	public string LastName { get; set; }
	// 	public string Email { get; set; }
	// 	public string Mobile { get; set; }
	// 	public string Country { get; set; }
	// 	public int mobile_verified { get; set; }

	// 	public string lid { get; set; }

	// 	public string ol_username { get; set; }

	// 	public string ol_password { get; set; }

	// 	/// <summary>
	// 	/// User balance usd
	// 	/// </summary>
	// 	public string customfield54 { get; set; }

	// 	public decimal customfield57 { get; set; }

	// 	/// <summary>
	// 	/// User balance local
	// 	/// </summary>
	// 	public string customfield82 { get; set; }

	// 	public string customfield72 { get; set; }

	// 	public string customfield109 { get; set; }
	// 	public string customfield104 { get; set; }
	// 	public decimal customfield55 { get; set; }
	// 	public int affiliateID { get; set; }
	// 	public string Address { get; set; }
	// 	public string Zip { get; set; }
	// 	public string State { get; set; }
	// 	public string City { get; set; }
	// 	public bool email_verified { get; set; }
	// 	public decimal customfield178 { get; set; }
	// 	public decimal customfield185 { get; set; }
	// }

	
	// public static async Task UpdateUserSignUpDataAsync(int entityId, string langIso, string signUpDomain,
	// 		string signUpIp, string referer)
	// {
	// 	var config = EnvironmentHelper.BusinessApiConfiguration;

	// 	var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

	// 	var loginSignUpDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

	// 	var apiRequest = new Entity_UpdateRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		EntityId = entityId,
	// 		NamesArray = new[]
	// 			{
	//               /*"CustomField3", */"CustomField9", "CustomField68", "CustomField67", "CustomField10", "CustomField4",
	// 									"CustomField71", "categoryID"
	// 							},
	// 		ValuesArray = new[]
	// 			{
	//               /*loginSignUpDate, */signUpIp, signUpDomain, langIso, signUpIp, loginSignUpDate,
	// 									string.IsNullOrEmpty(referer) ? "" : referer, "60"
	// 							}
	// 	};

	// 	var apiResponse = await client.Entity_UpdateAsync(apiRequest);
	// }

	// public static FileModel? GetFile(int id)
	// {
	// 	var user = WebStorageUtility.GetSignedUser();

	// 	if (user is null)
	// 		return null;

	// 	var config = EnvironmentHelper.BusinessApiConfiguration;

	// 	var client = config.InitClient();

	// 	var apiRequest = new Entity_Files_GetRequest
	// 	{
	// 		ol_EntityID = config.ol_EntityId,
	// 		ol_Username = config.ol_UserName,
	// 		ol_Password = config.ol_Password,
	// 		BusinessId = config.BusinessId,
	// 		Fields = new[] { "Filename", "FileData" },
	// 		FilterFields = new[] { "fileid", "EntityId" },
	// 		FilterValues = new[] { $"{id}", $"{user.EntityId}" }
	// 	};

	// 	var apiResponse = client.Entity_Files_Get(apiRequest);

	// 	var response = JsonConvert.DeserializeObject<List<FileModel>>(apiResponse.@return)?.FirstOrDefault();

	// 	if (response is null)
	// 		return null;

	// 	response.Base64ImgString = Convert.ToBase64String(response.ByteArrayImg);

	// 	return response;
	// }


//}



// public class EntityFileResponse : IdentityHelper.GeneralApiResponse
// {
// 	[JsonProperty("fileid")] public int FileId { get; set; }
// }