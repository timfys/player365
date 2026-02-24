using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;

namespace GoldCasino.ApiModule.Services;

public class UserService(IBusinessApiService businessApi, ILogger<UserService> logger)
{
	public async Task<Result<(UserDto, UserBalanceDto), Error>> GetMeAsync(UserApiAccess access)
	{
		try
		{
			var result = await businessApi.EntityFindAsync(new()
			{
				Fields = FieldHelper<UserDto, UserBalanceDto>.Fields,
				Filter = new() { { "entityId", $"{access?.EntityId}" } }
			}, access);


			if (result.Error != null)
				return Result<(UserDto, UserBalanceDto), Error>.Fail(result.Error);

			var entity = result.Value.Entities.FirstOrDefault();

			var (user, balance) = EntityMapper.MapTo<UserDto, UserBalanceDto>(entity);

			if (entity == null)
				return Result<(UserDto, UserBalanceDto), Error>.Fail(new Error(
						UserResultCodes.NotFound,
						$"Entity not found for user {access.EntityId}"
				));


			return Result<(UserDto, UserBalanceDto), Error>.Ok((user, balance));
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during GetMe for Entity {EntityId}", access.EntityId);
			throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during GetMe", ex);
		}
	}

	public async Task<Result<bool, Error>> Update(UserUpdateDto userDto, UserApiAccess access)
	{
		try
		{
			var result = await businessApi.EntityUpdateAsync(access.EntityId, userDto, access);

			if (result.IsSuccess)
				return result;

			if (result.Error is SoapApiError error)
				return error.RemoteCode switch
				{
					EntityUpdateResponse.ResultCodeEmailNotValid
							=> Result<bool, Error>.Fail(
									new Error(AuthResultCodes.InvalidEmail, "The provided email is not valid.")),

					EntityUpdateResponse.ResultCodeExists
							when error.Message?.Contains("email", StringComparison.OrdinalIgnoreCase) == true
							=> Result<bool, Error>.Fail(
									new Error(AuthResultCodes.EmailAlreadyRegistered, "The provided email is already registered.")),

					EntityUpdateResponse.ResultCodeExists
							when error.Message?.Contains("mobile", StringComparison.OrdinalIgnoreCase) == true
							=> Result<bool, Error>.Fail(
									new Error(AuthResultCodes.InvalidPhoneNumber, "The provided phone number is already registered.")),

					_ => Result<bool, Error>.Fail(result.Error)
				};

			return result;
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during Update for Entity {EntityId}", access.EntityId);
			throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during Update", ex);
		}
	}

	public async Task<Result<bool, Error>> VerifyEmailAsync(VerifyEmailDto verifyEmailDto, UserApiAccess access)
	{
		try
		{
			var result = await businessApi.EntityVerifyContactInfoAsync(new()
			{
				Code = verifyEmailDto.Code,
				VerifyType = BusinessApi.Models.VerificationType.Email,
				EntityId = access.EntityId == 0 ? verifyEmailDto.EntityId : access.EntityId
			});

			if (result.Error != null)
				return Result<bool, Error>.Fail(result.Error);

			return result;
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during VerifyEmail for Entity {EntityId}", access.EntityId);
			throw new UpstreamServiceException(ErrorCodes.UpstreamError, "Unexpected error during VerifyEmail", ex);
		}
	}
}

class UserResultCodes
{
	public const string NotFound = "USER_NOT_FOUND";
}
