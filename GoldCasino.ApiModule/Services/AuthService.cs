using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Helpers;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.SmartWinnersApi;
using PhoneNumbers;

namespace GoldCasino.ApiModule.Services;

public class AuthService(IBusinessApiService businessApi, ISmartWinnersApiService smApi, ILogger<AuthService> logger)
{
	private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();
	public async Task<Result<OlLoginResult, Error>> SignInAsync(
		string username,
		string password,
		string? countryIso = null,
		string? ip = null)
	{
		try
		{
			var model = new OlLogin
			{
				Username = username,
				Password = password,
				IP = ip ?? string.Empty,
				Language = countryIso ?? "en",
				DeviceKind = DeviceKind.Web
			};

			var result = await businessApi.OlLoginAsync(model);

			if (result.Error is SoapApiError soapError && soapError.RemoteCode == -1)
			{
				return Result<OlLoginResult, Error>.Fail(new Error(
						AuthResultCodes.InvalidCredentials,
					 "Invalid username or password"
				));
			}

			return result;
		}
		catch (AuthenticationServiceException)
		{
			// Let authentication faults bubble up for centralized handling (401)
				return Result<OlLoginResult, Error>.Fail(new Error(
						AuthResultCodes.InvalidCredentials,
					 "Invalid username or password"
				));
		}
		catch (UpstreamServiceException)
		{
			// Let upstream/parse/empty faults bubble up for centralized handling (502)
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during login: {Message}", ex.Message);
			// Wrap any unexpected error as upstream to be handled uniformly
			throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during login", ex);
		}
	}

	public async Task<Result<OlLoginResult, Error>> SignInAsync(string lid)
	{
		var result = (await businessApi.GeneralDecrypt(lid)).Value;
		if (result is not null)
			return await SignInAsync(result.Username, result.Password);
		throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during login with lid: lid decryption returned null");
	}

	public async Task<Result<SignWithPhoneResult, Error>> SignInWithPhone(string phone, string? code, int affiliateEntityId = 0)
	{
		try
		{
			var phoneNumber = PhoneHelper.Parse(phone);

			if (phoneNumber == null || !_phoneUtil.IsValidNumber(phoneNumber))
				return Result<SignWithPhoneResult, Error>.Fail(new Error(
					 AuthResultCodes.InvalidPhoneNumber,
					 "Invalid phone number"
				));

			var iso = _phoneUtil.GetRegionCodeForNumber(phoneNumber);
			var result = await smApi.SignInWithPhoneAsync(new SignWithPhone
			{
				Phone = phoneNumber.NationalNumber.ToString(),
				CountryISO = iso,
				Code = code,
				AffiliateEntityId = affiliateEntityId
			});


			if (result.Error is SoapApiError soapError)
			{
				switch (soapError.RemoteCode)
				{
					case -13: // Member registered
						return Result<SignWithPhoneResult, Error>.Fail(new Error(
							 AuthResultCodes.PhoneAlreadyRegistered,
							 "Phone number is already registered"
						));
					case -14:
						return Result<SignWithPhoneResult, Error>.Fail(new Error(
								 AuthResultCodes.MessageNotFound,
								 "Verification message not found"
							));
					case -15: // Invalid code
						return Result<SignWithPhoneResult, Error>.Fail(new Error(
							 AuthResultCodes.InvalidVerificationCode,
							 "Invalid verification code"
						));
					case -16: // Mobile not found
						return Result<SignWithPhoneResult, Error>.Fail(new Error(
							 AuthResultCodes.MobileNotFound,
							 "Phone number not found"
						));
					default:
						break;
				}

				return Result<SignWithPhoneResult, Error>.Fail(soapError);
			}
			return result;
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during login: {Message}", ex.Message);
			throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during login", ex);
		}
	}

	public async Task<Result<ForgotPasswordResult, Error>> ForgotPasswordAsync(ForgotPasswordCommand cmd)
	{
		try
		{
			var lang = (cmd.Language is "en" or "he") ? cmd.Language! : "en";

			var token = !string.IsNullOrWhiteSpace(cmd.Token) && !string.Equals(cmd.Token, "null", StringComparison.OrdinalIgnoreCase)
				? cmd.Token!
				: string.Empty;

			var newPwd = !string.IsNullOrWhiteSpace(cmd.NewPassword) && !string.Equals(cmd.NewPassword, "null", StringComparison.OrdinalIgnoreCase)
				? cmd.NewPassword!
				: string.Empty;

			var inboxId = string.Equals(cmd.CountryIso, "IL", StringComparison.OrdinalIgnoreCase) ? 26 : 16;

			var resetLink = $"{cmd.HostBase.TrimEnd('/')}/set-password?t=(!hash!)&u=(!ol_username!)";

			var request = new EntityForgotPassword
			{
				RemindKind = cmd.RemindKind,
				Language = lang,
				Username = cmd.Username,
				TokenCode = token,
				NewPassword = newPwd,
				Domain = $"<SHORTURL domain=\"{cmd.ShortUrlDomain}\">{resetLink}</SHORTURL>",
				InboxId = inboxId
			};

			var apiResult = await businessApi.EntityForgotPasswordAsync(request);

			if (apiResult.Error is SoapApiError soapError)
			{
				if (soapError.RemoteCode == -14)
					return Result<ForgotPasswordResult, Error>.Fail(
						new Error(AuthResultCodes.UserNotFound, "User not found"));

				return Result<ForgotPasswordResult, Error>.Fail(soapError);
			}

			var status = string.IsNullOrEmpty(token)
				? (cmd.RemindKind == RemindKind.SMS ? ForgotPasswordStatus.InitiatedSms : ForgotPasswordStatus.InitiatedWhatsApp)
				: ForgotPasswordStatus.PasswordChanged;

			return Result<ForgotPasswordResult, Error>.Ok(new ForgotPasswordResult(status, cmd.Username));
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during forgot password for {User}: {Message}", cmd.Username, ex.Message);
			throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during forgot password", ex);
		}
	}

	public sealed record ForgotPasswordCommand(
		string Username,
		RemindKind RemindKind,                 // 1 = SMS, иначе WhatsApp (как в старом коде)
		string? CountryIso,              // "IL" => InboxId = 26, иначе 16
		string HostBase,                // например: "https://playerclub.app" (без завершающего '/')
		string? Token = null,           // если пусто => отправка кода; если есть => смена пароля
		string? NewPassword = null,
		string? Language = null,        // принимаем любой; сервис сам задефолтит на "en"/"he"
		string ShortUrlDomain = "s.playerclub.app"  // как было в контроллере
	);
	public enum ForgotPasswordStatus
	{
		InitiatedSms,
		InitiatedWhatsApp,
		PasswordChanged
	}
	public sealed record ForgotPasswordResult(
	ForgotPasswordStatus Status,
	string Username
);

	//public async Task<Result<bool, Error>> SendPhoneVerificationCodeAsync(string phoneNumber, string? ip)
	//{
	//	try
	//	{
	//		var model = new OlSendPhoneVerificationCode
	//		{
	//			PhoneNumber = phoneNumber,
	//			IP = ip ?? string.Empty
	//		};
	//		var result = await businessApi.OlSendPhoneVerificationCodeAsync(model);
	//		if (!result.IsSuccess) return Result<bool, Error>.Fail(result.Error!);
	//		return Result<bool, Error>.Ok(true);
	//	}
	//	catch (UpstreamServiceException)
	//	{
	//		// Let upstream/parse/empty faults bubble up for centralized handling (502)
	//		throw;
	//	}
	//	catch (Exception ex)
	//	{
	//		logger.LogError(ex, "Unexpected error during sending phone verification code: {Message}", ex.Message);
	//		// Wrap any unexpected error as upstream to be handled uniformly
	//		throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during sending phone verification code", ex);
	//	}
	//}

	//public async Task<Result<bool, Error>> SignOutAsync(string entityId, string? lid)
	//{
	//	try
	//	{
	//		var model = new OlLogout
	//		{
	//			EntityId = int.TryParse(entityId, out var id) ? id : 0,
	//			Lid = lid ?? string.Empty
	//		};
	//		var result = await businessApi.OlLogoutAsync(model);
	//		if (!result.IsSuccess) return Result<bool, Error>.Fail(result.Error!);
	//		return Result<bool, Error>.Ok(true);
	//	}
	//	catch (UpstreamServiceException)
	//	{
	//		// Let upstream/parse/empty faults bubble up for centralized handling (502)
	//		throw;
	//	}
	//	catch (Exception ex)
	//	{
	//		logger.LogError(ex, "Unexpected error during logout: {Message}", ex.Message);
	//		// Wrap any unexpected error as upstream to be handled uniformly
	//		throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during logout", ex);
	//	}
	//}

}