using System.Threading.Tasks;
using GoldCasino.ApiModule.Auth;
using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Services;
using Microsoft.AspNetCore.Authorization;
using PhoneNumbers;
using Serilog;

namespace GoldCasino.ApiModule.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserService userService, AuthService authService, IAuthCookieService authCookie, ILogger<UserController> logger) : ControllerBase
{
	private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

	[HttpGet]
	[Authorize(AuthenticationSchemes = AuthDefaults.EncryptedCookieScheme)]
	public async Task<IActionResult> GetUserInfo()
	{
		var auth = HttpContext.User.ToUserApiAccess();
		var resp = await userService.GetMeAsync(auth);

		if (resp.IsSuccess)
			return Ok(resp.Value.Item1);

		logger.LogWarning("Failed to get user info for EntityId {EntityId}. Code={Code}, Message={Message}", auth.EntityId, resp.Error?.Code, resp.Error?.Message);
		return Problem();
	}

	[HttpPatch]
	[Authorize(AuthenticationSchemes = AuthDefaults.EncryptedCookieScheme)]
	public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto dto)
	{
		try
		{
			var user = HttpContext.User.ToUserApiAccess();

			// If phone is provided, normalize it and sync Username to the normalized phone (E.164)
			if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
				try
				{
					var raw = dto.PhoneNumber!.Trim();
					var region = dto.CountryIso?.Trim().ToUpperInvariant();
					PhoneNumber parsed;
					if (raw.StartsWith('+'))
						parsed = _phoneUtil.Parse(raw, "ZZ");
					else if (!string.IsNullOrWhiteSpace(region))
						parsed = _phoneUtil.Parse(raw, region);
					else
						parsed = _phoneUtil.Parse(raw, "ZZ");

					if (_phoneUtil.IsValidNumber(parsed))
					{
						var e164 = _phoneUtil.Format(parsed, PhoneNumberFormat.E164);
						dto.Username = e164.Trim('+');
						dto.PhoneNumber = parsed.NationalNumber.ToString();
					}
					else
						logger.LogWarning("Provided phone number is not valid. Phone={Phone}, CountryIso={CountryIso}, EntityId={EntityId}", dto.PhoneNumber, dto.CountryIso, user?.EntityId);

					
				}
				catch (NumberParseException ex)
				{
					logger.LogWarning(ex, "Failed to parse phone number for user update. Phone={Phone}, CountryIso={CountryIso}, EntityId={EntityId}", dto.PhoneNumber, dto.CountryIso, user?.EntityId);
				}

			var result = await userService.Update(dto, user);

			if (result.IsSuccess)
			{
				// If sensitive auth fields changed, re-login and re-issue cookie seamlessly
				var shouldRelogin = !string.IsNullOrWhiteSpace(dto.Password) || !string.IsNullOrWhiteSpace(dto.Username);
				if (shouldRelogin && user is not null)
				{
					var newUsername = string.IsNullOrWhiteSpace(dto.Username) ? user.Username : dto.Username!;
					var newPassword = string.IsNullOrWhiteSpace(dto.Password) ? user.Password : dto.Password!;

					var loginResult = await authService.SignInAsync(newUsername, newPassword, null, GetClientIP());
					if (loginResult.IsSuccess && loginResult.Value is not null)
					{
						var data = loginResult.Value;
						var credential = new Credential(
							EntityId: data.EntityId,
							Username: newUsername,
							Password: newPassword,
							Lid: data.Lid
						);

						authCookie.Set(credential);
					}
					else if (!loginResult.IsSuccess)
					{
						// If re-login failed after password/username update, surface a clear error
						var err = loginResult.Error!;
						logger.LogWarning("Relogin failed after user update for EntityId {EntityId}. Code={Code}, Message={Message}", user.EntityId, err.Code, err.Message);
						if (err.Code == AuthResultCodes.InvalidCredentials)
						{
							var problem = new ProblemDetails
							{
								Status = StatusCodes.Status401Unauthorized,
								Title = "Invalid credentials after update",
								Type = "https://httpstatuses.com/401",
								Detail = err.Message
							};
							problem.Extensions["code"] = err.Code;
							problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
							return Unauthorized(problem);
						}

						// For other unexpected errors, return 500
						var problemUnexpected = new ProblemDetails
						{
							Status = StatusCodes.Status500InternalServerError,
							Title = "Relogin failed",
							Type = "https://httpstatuses.com/500",
							Detail = err.Message
						};
						problemUnexpected.Extensions["code"] = err.Code;
						problemUnexpected.Extensions["traceId"] = HttpContext.TraceIdentifier;
						return StatusCode(StatusCodes.Status500InternalServerError, problemUnexpected);
					}
				}

				return Ok(new { message = "User information updated successfully." });
			}

			if(result.Error != null){
				logger.LogWarning("Failed to update user info for EntityId {EntityId}. Code={Code}, Message={Message}", user?.EntityId, result.Error.Code, result.Error.Message);

				return BadRequest(new
				{
					code = result.Error.Code,
					message = result.Error.Message
				});
			}

			return BadRequest(new
			{
				code = AuthResultCodes.UnexpectedError,
				message = "Failed to update user information"
			});
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error while updating user info for EntityId {EntityId}", HttpContext.User.ToUserApiAccess()?.EntityId);
			var problem = new ProblemDetails
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "Internal Server Error",
				Type = "https://httpstatuses.com/500",
				Detail = "An unexpected error occurred."
			};
			problem.Extensions["code"] = AuthResultCodes.UnexpectedError;
			problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
			return StatusCode(StatusCodes.Status500InternalServerError, problem);
		}
	}

	[HttpPost("verify-email")]
	public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
	{
		try
		{
			var user = HttpContext.User.ToUserApiAccess();
			var result = await userService.VerifyEmailAsync(dto, user);

			if (result.IsSuccess)
				return Ok(new { message = "Email verified successfully." });

			return BadRequest(new
			{
				code = result.Error?.Code ?? AuthResultCodes.UnexpectedError,
				message = result.Error?.Message ?? "Failed to verify email."
			});
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error while verifying email for EntityId {EntityId}", HttpContext.User.ToUserApiAccess()?.EntityId);
			var problem = new ProblemDetails
			{
				Status = StatusCodes.Status500InternalServerError,
				Title = "Internal Server Error",
				Type = "https://httpstatuses.com/500",
				Detail = "An unexpected error occurred."
			};
			problem.Extensions["code"] = AuthResultCodes.UnexpectedError;
			problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
			return StatusCode(StatusCodes.Status500InternalServerError, problem);
		}
	}

	private string GetClientIP()
	{
		return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	}
}
