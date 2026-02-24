using GoldCasino.ApiModule.Common.Patching;
using GoldCasino.ApiModule.Dtos.Bonuses;
using GoldCasino.ApiModule.Dtos.Files;
using GoldCasino.ApiModule.Integrations.BusinessApi;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.BusinessApi.Policies;
using Entity_ForgotpasswordRequest = GoldCasino.ApiModule.Integrations.BusinessApi.Entity_ForgotpasswordRequest;
using Entity_ForgotpasswordResponse = GoldCasino.ApiModule.Integrations.BusinessApi.Entity_ForgotpasswordResponse;
using Entity_LogtrafficRequest = GoldCasino.ApiModule.Integrations.BusinessApi.Entity_LogtrafficRequest;
using Entity_VerifyContactInfoRequest = GoldCasino.ApiModule.Integrations.BusinessApi.Entity_VerifyContactInfoRequest;

namespace GoldCasino.ApiModule.Services.BusinessApi;

internal sealed class BusinessApiService(
IOptions<BusinessApiOptions> options,
IBusinessAPI client,
ILogger<BusinessApiService> logger) : SoapServiceBase, IBusinessApiService
{

	public async Task<Result<EntityFindResult, Error>> EntityFindAsync(
			EntityFind model,
			UserApiAccess? accessData = null)
	{
		var req = BuildEntityFindRequest(model, accessData);

		var raw = await ExecuteAsync<List<Entity>>(
				async () => (await client.Entity_FindAsync(req)).@return);

		if (!raw.IsSuccess) return Result<EntityFindResult, Error>.Fail(raw.Error!);

		return Result<EntityFindResult, Error>.Ok(
				new() { Entities = raw.Value! });
	}

	public async Task<Result<bool, Error>> EntityUpdateAsync(Action<Patch<Entity>> configure, ImagePatch? images, UserApiAccess? accessData = null)
	{
		var req = BuildEntityUpdateRequest(configure, images, accessData);

		var raw = await ExecuteAsync<EntityUpdateResponse>(
				async () => (await client.Entity_UpdateAsync(req)).@return);

		if (!raw.IsSuccess) return Result<bool, Error>.Fail(raw.Error!);

		return Result<bool, Error>.Ok(true);
	}

	public async Task<Result<bool, Error>> EntityUpdateAsync<TDto>(
		int entityId,
		TDto dto,
		UserApiAccess? accessData = null)
	{
		var (names, values) = FlattenDtoToSoapArrays(dto!, SoapUpdatePolicies.Entity);

		if (names.Length == 0)
			return Result<bool, Error>.Ok(true);

		var master = options.Value.Credentials;

		var req = new Entity_UpdateRequest
		{
			ol_EntityID = accessData?.EntityId ?? master.EntityId,
			ol_Username = accessData?.Username ?? master.Username,
			ol_Password = accessData?.Password ?? master.Password,

			EntityId = entityId,
			NamesArray = names,
			ValuesArray = values,

			// Images are specific to Entity; default to empty
			ImageFields = [],
			ImageValues = []
		};

		var raw = await ExecuteAsync<EntityUpdateResponse>(
				async () => (await client.Entity_UpdateAsync(req)).@return);

		if (!raw.IsSuccess) return Result<bool, Error>.Fail(raw.Error!);

		return Result<bool, Error>.Ok(true);
	}

	public async Task<Result<bool, Error>> EntityUpdateAsync<TDto, TImgDto>(
		int entityId,
		TDto fieldsDto,
		TImgDto? imagesDto,
		UserApiAccess? accessData = null)
	{
		var (names, values) = FlattenDtoToSoapArrays(fieldsDto!, SoapUpdatePolicies.Entity);

		string[] imgNames = [], imgValues = [];

		if (imagesDto is not null)
			(imgNames, imgValues) = FlattenDtoToSoapArrays(imagesDto, SoapUpdatePolicies.Entity);

		var master = options.Value.Credentials;
		var req = new Entity_UpdateRequest
		{
			ol_EntityID = accessData?.EntityId ?? master.EntityId,
			ol_Username = accessData?.Username ?? master.Username,
			ol_Password = accessData?.Password ?? master.Password,
			EntityId = entityId,
			NamesArray = names,
			ValuesArray = values,
			ImageFields = imgNames,
			ImageValues = imgValues
		};

		var raw = await ExecuteAsync<EntityUpdateResponse>(
				async () => (await client.Entity_UpdateAsync(req)).@return);

		if (!raw.IsSuccess) return Result<bool, Error>.Fail(raw.Error!);
		return Result<bool, Error>.Ok(true);
	}

	public async Task<Result<OlLoginResult, Error>> OlLoginAsync(OlLogin model)
	{
		var req = BuildOlLoginRequest(model);

		var raw = await ExecuteAsync<OlLoginResponse>(
				async () => (await client.Ol_loginAsync(req)).@return);

		if (!raw.IsSuccess) return Result<OlLoginResult, Error>.Fail(raw.Error!);

		var r = raw.Value!;
		return Result<OlLoginResult, Error>.Ok(new OlLoginResult
		{
			EntityId = r.EntityId.ToString(),
			Username = model.Username,
			Password = model.Password,
			Lid = r.Lid ?? string.Empty
		});
	}

	public async Task<Result<EntityBonusesGetResult, Error>> EntityBonusesGetAsync(EntityBonusesGet model, UserApiAccess? accessData = null)
	{
		var req = BuildEntityBonusesGetRequest(model, accessData);

		var parsed = await ExecuteAsync<EntityBonusesGetResult>(
				async () => (await client.CustomFields_Tables_GetAsync(req)).@return);

		if (!parsed.IsSuccess) return Result<EntityBonusesGetResult, Error>.Fail(parsed.Error!);

		return Result<EntityBonusesGetResult, Error>.Ok(new()
		{
			Data = parsed.Value?.Data ?? []
		});
	}

	public async Task<Result<EntityAddResult, Error>> EntityAddAsync(EntityAdd model, UserApiAccess? accessData = null)
	{
		var req = BuildEntityAddRequest(model, accessData);

		var parsed = await ExecuteAsync<EntityAddResponse>(
			async () => (await client.Entity_AddAsync(req)).@return);

		if (!parsed.IsSuccess) return Result<EntityAddResult, Error>.Fail(parsed.Error!);

		var resp = parsed.Value!;
		var result = new EntityAddResult
		{
			EntityId = resp.EntityId,
			Username = resp.Username,
			AffiliateResultCode = resp.AffiliateResultCode,
			CustomerExists = resp.IsCustomerExists()
		};

		return Result<EntityAddResult, Error>.Ok(result);
	}

	public async Task<Result<EntityFilesUpdateResponse, Error>> EntityFilesUpdateAsync(EntityFilesUpdate model, UserApiAccess? accessData = null)
	{
		var req = BuildEntityFilesUpdateRequest(model, accessData);

		var parsed = await ExecuteAsync<EntityFilesUpdateResponse>(
				async () => (await client.Entity_Files_UpdateAsync(req)).@return);

		if (!parsed.IsSuccess) return Result<EntityFilesUpdateResponse, Error>.Fail(parsed.Error!);

		return Result<EntityFilesUpdateResponse, Error>.Ok(parsed.Value!);
	}

	public async Task<Result<EntityFilesGetResult, Error>> EntityFilesGetAsync(EntityFilesGet model, UserApiAccess? accessData = null)
	{
		var req = BuildEntityFilesGetRequest(model, accessData);

		var parsed = await ExecuteAsync<List<EntityFile>>(
				async () => (await client.Entity_Files_GetAsync(req)).@return);

		if (!parsed.IsSuccess) return Result<EntityFilesGetResult, Error>.Fail(parsed.Error!);

		return Result<EntityFilesGetResult, Error>.Ok(new()
		{
			Files = parsed.Value ?? []
		});
	}

	public async Task<Result<bool, Error>> EntityLogTrafficAsync(EntityLogTraffic model, UserApiAccess? accessData = null)
	{
		var req = BuildEntityLogTrafficRequest(model, accessData);

		var parsed = await ExecuteAsync<EntityLogTrafficResponse>(
				async () => (await client.Entity_LogtrafficAsync(req)).@return);

		if (!parsed.IsSuccess) return Result<bool, Error>.Fail(parsed.Error!);

		return Result<bool, Error>.Ok(true);
	}

	public async Task<Result<bool, Error>> EntityVerifyContactInfoAsync(EntityVerifyContactInfo model, UserApiAccess? accessData = null)
	{
		var master = options.Value.Credentials;
		var req = new Entity_VerifyContactInfoRequest
		{
			ol_EntityID = accessData?.EntityId ?? master.EntityId,
			ol_Username = accessData?.Username ?? master.Username,
			ol_Password = accessData?.Password ?? master.Password,
			entityID = model.EntityId,
			VerifyType = (int)model.VerifyType,
			VerificationCode = model.Code ?? string.Empty,
			businessId = 3,
			NamesArray = model.AdditionalData?.Keys.ToArray() ?? [],
			ValuesArray = model.AdditionalData?.Values.ToArray() ?? []
		};

		var result = await ExecuteAsync<EntityVerifyContactInfoResponse>(
			async () => (await client.Entity_VerifyContactInfoAsync(req)).@return);

		if (!result.IsSuccess) return Result<bool, Error>.Fail(result.Error!);

		return Result<bool, Error>.Ok(true);
	}

	public async Task<Result<string, Error>> OutgoingAddAsync(OutgoingAdd model, UserApiAccess? accessData = null)
	{
		var req = BuildOutgoingAddRequest(model, accessData);

		var result = await ExecuteAsync<ApiResponse>(
			async () => (await client.Outgoing_addAsync(req)).@return);

		if (!result.IsSuccess) return Result<string, Error>.Fail(result.Error!);

		return Result<string, Error>.Ok(result.Value?.ResultMessage ?? string.Empty);
	}

	public async Task<Result<GeneralDecrypt, Error>> GeneralDecrypt(string lid)
	{
		var req = BuildGeneralDecryptRequest(lid);
		var raw = await ExecuteAsync<GeneralDecrypt>(
				async () => (await client.General_DecryptAsync(req)).@return);
		if (!raw.IsSuccess) return Result<GeneralDecrypt, Error>.Fail(raw.Error!); // probaly will never happen
		return Result<GeneralDecrypt, Error>.Ok(raw.Value!);
	}

	public async Task<Result<bool, Error>> EntityForgotPasswordAsync(EntityForgotPassword model)
	{
		var req = BuildEntityForgotPasswordRequest(model);

		var raw = await ExecuteAsync<Entity_ForgotpasswordResponse>(
				() => client.Entity_ForgotpasswordAsync(req).ContinueWith(t => t.Result.@return));

		if (!raw.IsSuccess) return Result<bool, Error>.Fail(raw.Error!);

		return Result<bool, Error>.Ok(true);
	}

	#region Request Builders
	private Entity_FindRequest BuildEntityFindRequest(
					EntityFind model,
					UserApiAccess? access)
	{
		var master = options.Value.Credentials;

		var req = new Entity_FindRequest
		{
			ol_EntityId = access?.EntityId ?? master.EntityId,
			ol_UserName = access?.Username ?? master.Username,
			ol_Password = access?.Password ?? master.Password,
			BusinessId = master.BusinessId,
			LimitCount = model.LimitCount ?? 0,
			LimitFrom = model.LimitFrom ?? 0,
			Limit_entities_per_business = model.LimitEntitiesPerBusiness ?? false
		};

		// — filters -----------------------------------------------------------
		if (model.Filter?.Count > 0)
		{
			req.FilterFields = [.. model.Filter.Keys];
			req.FilterValues = [.. model.Filter.Values];
		}

		// — sample fields ------------------------------------------------------
		req.Fields = model.Fields is { Length: > 0 }
									? model.Fields
									: ["EntityID", "Email", "FirstName", "LastName"];

		return req;
	}

	private Entity_UpdateRequest BuildEntityUpdateRequest(
			Action<Patch<Entity>> configure,
			ImagePatch? images,
			UserApiAccess? access)
	{
		var master = options.Value.Credentials;
		var patch = new Patch<Entity>();
		configure(patch);
		var (names, values) = patch.ToArrays();

		var req = new Entity_UpdateRequest
		{
			ol_EntityID = access?.EntityId ?? master.EntityId,
			ol_Username = access?.Username ?? master.Username,
			ol_Password = access?.Password ?? master.Password,
			NamesArray = names,
			ValuesArray = values
		};

		if (images is not null && !images.IsEmpty)
		{
			var (iNames, iValues) = images.ToArrays();
			req.ImageFields = iNames;
			req.ImageValues = iValues;
		}

		return req;
	}

	private static Ol_loginRequest BuildOlLoginRequest(OlLogin model)
	{
		return new Ol_loginRequest
		{
			UserName = model.Username,
			Password = model.Password,
			IP = model.IP,
			Language = model.Language,
			DeviceKind = (int)model.DeviceKind,
			SystemInfo = [],
			Token = string.Empty
		};
	}

	private Entity_AddRequest BuildEntityAddRequest(EntityAdd model, UserApiAccess? access)
	{
		var master = options.Value.Credentials;
		return new Entity_AddRequest
		{
			ol_EntityId = access?.EntityId ?? master.EntityId,
			ol_UserName = access?.Username ?? master.Username,
			ol_Password = access?.Password ?? master.Password,
			BusinessId = master.BusinessId,
			Employee_EntityId = model.EmployeeEntityId,
			CategoryID = model.CategoryId,
			Email = model.Email,
			Password = model.Password,
			FirstName = model.FirstName,
			LastName = model.LastName,
			Mobile = model.Mobile,
			CountryISO = model.CountryISO,
			affiliate_entityID = model.AffiliateEntityId
		};
	}

	private static General_DecryptRequest BuildGeneralDecryptRequest(string lid)
	{
		return new General_DecryptRequest
		{
			Identification = lid
		};
	}

	private static Entity_ForgotpasswordRequest BuildEntityForgotPasswordRequest(EntityForgotPassword model)
	{
		return new Entity_ForgotpasswordRequest
		{
			RemindKind = (int)model.RemindKind,
			Language = model.Language ?? "en",
			businessID = model.RemindKind == RemindKind.Whatsapp ? 1 : 3,
			ol_UserName = model.Username,
			NewPassword = model.NewPassword,
			TokenCode = model.TokenCode,
			NamesArray = ["domain", "InboxId"],
			ValuesArray = [model.Domain, model.InboxId.ToString()]
		};
	}

	private Outgoing_addRequest BuildOutgoingAddRequest(OutgoingAdd model, UserApiAccess? access)
	{
		var master = options.Value.Credentials;
		var timestamp = model.Timestamp ?? DateTimeOffset.UtcNow;
		var subject = "Game preload failed - prepayment required";
		var entityIds = model.EntityIds is { Length: > 0 } ids ? ids : [4];

		return new Outgoing_addRequest
		{
			ol_EntityID = master.EntityId,
			ol_Username = master.Username,
			ol_Password = master.Password,
			MessageType = 3,
			MessageID = model.MessageId,
			Priority = model.Priority,
			EntityIds = entityIds,
			From = model.From,
			ScheduleTo = model.ScheduleTo,
			Order_DocumentID = model.OrderDocumentId,
			Destination = model.Destination,
			Body = BuildOutgoingAddBody(model, timestamp),
			NamesArray = ["subject"],
			ValuesArray = [subject]
		};
	}

	private static string BuildOutgoingAddBody(OutgoingAdd model, DateTimeOffset timestamp)
	{
		return $@"A game on the site failed to load because a required prepayment was not completed.

Customer: ID: {model.EntityId} Name: {model.EntityName} Mobile:{model.EntityMobile}
Game: {model.GameName} {model.GameUrl}
Game ID: {model.GameId}
Error: Prepayment required before loading
Time: {timestamp:O}

Please review the prepayment status and take the necessary action to restore availability.

System Notification";
	}

	private Entity_Files_UpdateRequest BuildEntityFilesUpdateRequest(EntityFilesUpdate model, UserApiAccess? access)
	{
		var master = options.Value.Credentials;
		var (names, values) = FlattenDtoToSoapArrays(model, SoapUpdatePolicies.Default);

		return new Entity_Files_UpdateRequest
		{
			ol_EntityID = access?.EntityId ?? master.EntityId,
			ol_Username = access?.Username ?? master.Username,
			ol_Password = access?.Password ?? master.Password,
			FileId = model.FileId,
			BusinessId = model.BusinessId ?? master.BusinessId,
			NamesArray = names,
			ValuesArray = values,
			FileData = model.FileData ?? []
    };
	}

	private Entity_Files_GetRequest BuildEntityFilesGetRequest(EntityFilesGet model, UserApiAccess? access)
	{
		var master = options.Value.Credentials;
		var filterFields = new List<string>();
		var filterValues = new List<string>();

		if (model.Filter?.Count > 0)
		{
			foreach (var pair in model.Filter)
			{
				filterFields.Add(pair.Key);
				filterValues.Add(pair.Value);
			}
		}

		if (!filterFields.Exists(static field => field.Equals("Order By", StringComparison.OrdinalIgnoreCase)))
		{
			filterFields.Add("Order By");
			filterValues.Add("Date desc");
		}

		var req = new Entity_Files_GetRequest
		{
			ol_EntityID = access?.EntityId ?? master.EntityId,
			ol_Username = access?.Username ?? master.Username,
			ol_Password = access?.Password ?? master.Password,
			BusinessId = model.BusinessId ?? master.BusinessId,
			LimitFrom = model.LimitFrom ?? 0,
			LimitCount = model.LimitCount ?? 0,
			Fields = model.Fields is { Length: > 0 } ? model.Fields : FieldHelper<EntityFileMinDto>.Fields,
			FilterFields = [.. filterFields],
			FilterValues = [.. filterValues]
    };

		return req;
	}

		private CustomFields_Tables_GetRequest BuildEntityBonusesGetRequest(EntityBonusesGet model, UserApiAccess? access)
		{
			var master = options.Value.Credentials;
			return new CustomFields_Tables_GetRequest
			{
				ol_EntityID = access?.EntityId ?? master.EntityId,
				ol_Username = access?.Username ?? master.Username,
				ol_Password = access?.Password ?? master.Password,
				TableID = 199,
				ParentRecordID = access?.EntityId ?? 0,
				Fields = model.Fields is { Length: > 0 } ? model.Fields : FieldHelper<EntityBonusDto>.Fields,
				FilterFields = model.Filter?.Keys.ToArray() ?? [],
				FilterValues = model.Filter?.Values.ToArray() ?? [],
				LimitFrom = model.LimitFrom ?? 0,
				LimitCount = model.LimitCount ?? 0
			};
		}

	private Entity_LogtrafficRequest BuildEntityLogTrafficRequest(EntityLogTraffic model, UserApiAccess? access)
	{
		var master = options.Value.Credentials;
		var entityId = model.EntityId ?? access?.EntityId ?? master.EntityId;
		var names = Array.Empty<string>();
		var values = Array.Empty<string>();

		if (model.Fields is { Count: > 0 } fields)
		{
			names = new string[fields.Count];
			values = new string[fields.Count];
			var idx = 0;
			foreach (var pair in fields)
			{
				names[idx] = pair.Key;
				values[idx] = pair.Value;
				idx++;
			}
		}

		return new Entity_LogtrafficRequest
		{
			ol_EntityID = entityId,
			device = (int)model.DeviceKind,
			System = model.SystemInfo ?? string.Empty,
			IP = model.IpAddress ?? string.Empty,
			URL = model.Url ?? string.Empty,
			NamesArray = names,
			ValuesArray = values
		};
	}
	#endregion
}
