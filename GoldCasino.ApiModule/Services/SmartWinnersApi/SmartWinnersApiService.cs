using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Models;

namespace GoldCasino.ApiModule.Services.SmartWinnersApi;

internal class SmartWinnersApiService(
	IOptions<SmartWinnersOptions> options,
	ISmartWinners sm,
	ILogger<SmartWinnersApiService> logger) : SoapServiceBase, ISmartWinnersApiService
{
	public async Task<Result<SignWithPhoneResult, Error>> SignInWithPhoneAsync(SignWithPhone model) // aid - affiliate id
	{
		var req = BuildOlPhoneLoginRequest(model.Phone, model.CountryISO, model.Code, model.AffiliateEntityId);

		var raw = await ExecuteAsync<SignWithPhoneResponse>(
				() => sm.Signin_With_PhoneAsync(req).ContinueWith(t => t.Result.@return));
		if (!raw.IsSuccess) return Result<SignWithPhoneResult, Error>.Fail(raw.Error!);
		var r = raw.Value!;

		if (r.IsPhoneAlreadyRegistered())
			return Result<SignWithPhoneResult, Error>.Fail(new Error
			(
				AuthResultCodes.PhoneAlreadyRegistered,
				"Phone number is already registered"
			));
		if (r.IsCooldown())
			return Result<SignWithPhoneResult, Error>.Fail(new Error
				(
					AuthResultCodes.Cooldown,
					"Please wait before requesting another code"
				));

		return Result<SignWithPhoneResult, Error>.Ok(new SignWithPhoneResult
		{
			EntityId = r.EntityId,
			Lid = r.Lid,
			SmsSent = r.IsSmsSent()
		});
	}

	public async Task<Result<VouchersGetResult, Error>> VouchersGetAsync(VouchersGet model, UserApiAccess? accessData = null)
	{
		var req = BuildVouchersGetRequest(model, accessData);

		var parsed = await ExecuteAsync<VouchersGetResult>(
				async () => (await sm.Vouchers_GetAsync(req)).@return);

		if (!parsed.IsSuccess) return Result<VouchersGetResult, Error>.Fail(parsed.Error!);

		return Result<VouchersGetResult, Error>.Ok(new VouchersGetResult
		{
			Data = parsed.Value?.Data ?? []
		});
	}

	public async Task<Result<string, Error>> EntityBalanceCalcAsync(UserApiAccess accessData)
	{
		var req = new Entity_Balance_CalcRequest
		{
			ol_EntityID = accessData.EntityId,
			ol_Username = accessData.Username,
			ol_Password = accessData.Password
		};

		var response = await sm.Entity_Balance_CalcAsync(req);
		return Result<string, Error>.Ok(response.@return);
	}

	#region Request Builders

	private static Signin_With_PhoneRequest BuildOlPhoneLoginRequest(string phone, string countryISO, string? code, int affiliateEntityId)
	{
		return new Signin_With_PhoneRequest
		{
			PhoneNumber = phone,
			CountryISO = countryISO,
			VerificationCode = code,
			affiliate_entityID = affiliateEntityId
		};
	}

	private Vouchers_GetRequest BuildVouchersGetRequest(VouchersGet model, UserApiAccess? access)
	{
		return new Vouchers_GetRequest
		{
			ol_EntityID = access?.EntityId ?? 0,
			ol_Username = access?.Username ?? string.Empty,
			ol_Password = access?.Password ?? string.Empty,
			Fields = model.Fields is { Length: > 0 } ? model.Fields : FieldHelper<Voucher>.Fields,
			FilterFields = model.Filter?.Keys.ToArray() ?? [],
			FilterValues = model.Filter?.Values.ToArray() ?? [],
			LimitFrom = model.LimitFrom ?? 0,
			LimitCount = model.LimitCount ?? 0
		};
	}

	#endregion
}
