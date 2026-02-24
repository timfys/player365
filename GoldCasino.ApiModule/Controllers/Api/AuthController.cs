using GoldCasino.ApiModule.Auth;
using GoldCasino.ApiModule.Common.Exceptions;
using GoldCasino.ApiModule.Dtos.User;
using GoldCasino.ApiModule.Helpers;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Models.Auth;
using GoldCasino.ApiModule.Services;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using System.Globalization;
using System.Linq;
using System.Web;

namespace GoldCasino.ApiModule.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController(CookieEncryptionHelper cookieEncryption, AuthService authService, IBusinessApiService businessApiService, IAuthCookieService authCookie, ILogger<AuthController> logger) : ControllerBase
{
	private readonly IAuthCookieService _authCookie = authCookie;
	private const string DefaultUserCategoryId = "123";

	[HttpPost("phone")]
	public async Task<IActionResult> SignInWithPhone([FromBody] PhoneLoginRequest request)
	{
		try
		{
			var serviceResult = await authService.SignInWithPhone(request.Phone, null, request.AffiliateEntityId);

			var url = $"verify-phone?user={HttpUtility.UrlEncode(CryptoUtility.EncryptObject(request.Phone))}";
			if (!serviceResult.IsSuccess)
				switch (serviceResult.Error?.Code)
				{
					case AuthResultCodes.InvalidPhoneNumber:
						return BadRequest(new ProblemDetails()
						{
							Status = StatusCodes.Status400BadRequest,
							Title = "Invalid phone number",
							Type = "https://httpstatuses.com/400",
							Detail = serviceResult.Error.Message,
							Extensions = { ["code"] = serviceResult.Error.Code }
						});
					case AuthResultCodes.PhoneAlreadyRegistered:
						return Ok(new PhoneLoginResponse
						{
							Verify = false
						});
					case AuthResultCodes.Cooldown:
						return Ok(new
						{
							Verify = true,
							RedirectUrl = url,
							Cooldown = true,
							serviceResult.Error.Message
						});
				}

			var smsSent = serviceResult.Value!.SmsSent;

			if (smsSent)
			{
				if (serviceResult.Value is not null)
				{
					var signUpData = BuildUserSignUpData(includeSignupFields: true, requestedLanguage: null, clientIp: GetUserIp(HttpContext));
					await UpdateUserSignUpDataAsync(serviceResult.Value.EntityId, signUpData);
					await TrafficLogHelper.LogBeforeLoginVisitedPagesAsync(HttpContext, businessApiService, serviceResult.Value.EntityId);
				}

				return Ok(new PhoneLoginResponse
				{
					Verify = true,
					RedirectUrl = url
				});
			}

			return Ok(new PhoneLoginResponse
			{
				Verify = false
			});
		}
		catch (UpstreamServiceException)
		{
			// Let the global handler produce a 502
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error sending phone verification code: {Message}", ex.Message);
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

	[HttpPost("verify-phone")]
	public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneRequest request)
	{
		try
		{
			var serviceResult = await authService.SignInWithPhone(request.Phone, request.Code);

			if (!serviceResult.IsSuccess)
				return (serviceResult.Error?.Code) switch
				{
					AuthResultCodes.InvalidPhoneNumber => BadRequest(new ProblemDetails()
					{
						Status = StatusCodes.Status400BadRequest,
						Title = "Invalid phone number",
						Type = "https://httpstatuses.com/400",
						Detail = serviceResult.Error.Message,
						Extensions = { ["code"] = serviceResult.Error.Code }
					}),
					AuthResultCodes.InvalidVerificationCode => BadRequest(new ProblemDetails()
					{
						Status = StatusCodes.Status400BadRequest,
						Title = "Invalid phone number",
						Type = "https://httpstatuses.com/400",
						Detail = serviceResult.Error.Message,
						Extensions = { ["code"] = serviceResult.Error.Code }
					}),
					_ => BadRequest(new ProblemDetails()
					{
						Status = StatusCodes.Status400BadRequest,
						Title = "Verification failed",
						Type = "https://httpstatuses.com/400",
						Detail = serviceResult.Error?.Message,
						Extensions = { ["code"] = serviceResult.Error?.Code }
					}),
				};


			if (serviceResult.Value is not null)
			{
				TrafficLogHelper.ClearBeforeSignupVisitedPages(HttpContext);
				
				// Set the AfterSignUpFlag cookie to trigger affiliate script on next page load
				// This cookie is read by MasterPage.cshtml to render affiliate tracking scripts
				// and is then cleared via JavaScript after the script is rendered
				HttpContext.Response.Cookies.Append(
					"9u45ftn4598ny43895n49856yn4", // WebStorageUtility.AfterSignUpFlag
					"1",
					new CookieOptions { Path = "/", Expires = DateTimeOffset.UtcNow.AddDays(1) }
				);
			}

			return Ok(new VerifyPhoneResponse
			{
				IsVerified = true,
				Lid = serviceResult.Value?.Lid ?? string.Empty,
				Message = "Phone number verified successfully."
			}); 
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error sending phone verification code: {Message}", ex.Message);
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

	// [HttpPost("verify-email")]
	// public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
	// {
	// 	try
	// 	{
	// 		var serviceResult = await authService.VerifyEmailAsync(request.Token);

	// 		if (!serviceResult.IsSuccess)
	// 			return BadRequest(new ProblemDetails()
	// 			{
	// 				Status = StatusCodes.Status400BadRequest,
	// 				Title = "Email verification failed",
	// 				Type = "https://httpstatuses.com/400",
	// 				Detail = serviceResult.Error?.Message,
	// 				Extensions = { ["code"] = serviceResult.Error?.Code }
	// 			});

	// 		return Ok(new
	// 		{
	// 			Success = true,
	// 			Message = "Email verified successfully."
	// 		});
	// 	}
	// 	catch (UpstreamServiceException)
	// 	{
	// 		throw;
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		logger.LogError(ex, "Error verifying email: {Message}", ex.Message);
	// 		var problem = new ProblemDetails
	// 		{
	// 			Status = StatusCodes.Status500InternalServerError,
	// 			Title = "Internal Server Error",
	// 			Type = "https://httpstatuses.com/500",
	// 			Detail = "An unexpected error occurred."
	// 		};
	// 		problem.Extensions["code"] = AuthResultCodes.UnexpectedError;
	// 		problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
	// 		return StatusCode(StatusCodes.Status500InternalServerError, problem);
	// 	}
	// }

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] AuthLoginRequest request, [FromQuery] string? lid)
	{
		try
		{
			var clientIp = GetUserIp(HttpContext);

			if (!string.IsNullOrEmpty(lid))
			{
				var serviceResultLid = await authService.SignInAsync(lid);
				return await HandleSuccessfulLogin(serviceResultLid, request.Username, request.Password, request.Language, clientIp);
			}

			var serviceResult = await authService.SignInAsync(
					request.Username,
					request.Password,
					request.Language,
					clientIp
			);
			return await HandleSuccessfulLogin(serviceResult, request.Username, request.Password, request.Language, clientIp);
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error during login: {Message}", ex.Message);
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

	[HttpGet("login")]
	public async Task<IActionResult> Login([FromQuery] string lid)
	{
		try
		{
			if (!string.IsNullOrEmpty(lid))
			{
				var serviceResult = await authService.SignInAsync(lid);
				var u = serviceResult.Value?.Username ?? string.Empty;
				var p = serviceResult.Value?.Password ?? string.Empty;
				return await HandleSuccessfulLogin(serviceResult, u, p, null, GetUserIp(HttpContext));
			}

			return Unauthorized();
		}
		catch (UpstreamServiceException)
		{
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error during login: {Message}", ex.Message);
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

	[HttpPost("forgot")]
	public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
	{
		try
		{
			var clientIp = GetUserIp(HttpContext);

			var serviceResult = await authService.ForgotPasswordAsync(new(
				request.Username,
				request.RemindKind,
				request.CountryIso,
				HttpContext.Request.Host.Value,
				request.Token,
				request.NewPassword,
				request.Language,
				"s.playerclub.app"));

			if (serviceResult.IsSuccess)
			{
				if (!string.IsNullOrWhiteSpace(request.Token))
				{
					var loginResult = await authService.SignInAsync(
						request.Username,
						request.NewPassword,
						request.Language,
						clientIp
					);

					if (!loginResult.IsSuccess)
					{
						throw new Exception($"Password reset succeeded but auto-login failed: {loginResult.Error?.Message}");
					}

					await HandleSuccessfulLogin(loginResult, request.Username, request.NewPassword, request.Language, clientIp);
				}

				return Ok(new
				{
					Code = serviceResult.Value?.Status
				});
			}

			var err = serviceResult.Error!;
			if (err.Code == AuthResultCodes.UserNotFound)
			{
				var problem = new ProblemDetails
				{
					Status = StatusCodes.Status404NotFound,
					Title = "User not found",
					Type = "https://httpstatuses.com/404",
					Detail = err.Message
				};
				problem.Extensions["code"] = err.Code;
				problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
				return NotFound(problem);
			}
			throw new Exception($"Unexpected error during forgot password: {err.Message}");
		}
		catch (UpstreamServiceException)
		{
			// Let the global handler produce a 502
			throw;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error during forgot password: {Message}", ex.Message);
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

	[HttpGet("logout")]
	public IActionResult Logout()
	{
		try
		{
			_authCookie.Delete();

			return Ok(new AuthResponse
			{
				Success = true,
				Code = AuthResultCodes.LogoutSuccess
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error during logout: {Message}", ex.Message);
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

	[HttpGet("status")]
	public IActionResult GetAuthStatus()
	{
		try
		{
			if (HttpContext.User.Identity?.IsAuthenticated == true)
			{
				var entityId = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
				var username = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

				return Ok(new AuthStatusResponse
				{
					IsAuthenticated = true,
					EntityId = entityId,
					Username = username
				});
			}

			return Ok(new AuthStatusResponse
			{
				IsAuthenticated = false
			});
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error fetching auth status: {Message}", ex.Message);
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

	#region Helpers
	private async Task<IActionResult> HandleSuccessfulLogin(
			Result<OlLoginResult, Error> serviceResult,
			string username,
			string password,
			string? language,
			string? clientIp)
	{
		if (!serviceResult.IsSuccess)
		{
			var err = serviceResult.Error!;
			if (err.Code == AuthResultCodes.InvalidCredentials)
			{
				var problem = new ProblemDetails
				{
					Status = StatusCodes.Status401Unauthorized,
					Title = "Invalid credentials",
					Type = "https://httpstatuses.com/401",
					Detail = err.Message
				};
				problem.Extensions["code"] = err.Code;
				problem.Extensions["traceId"] = HttpContext.TraceIdentifier;

				return Unauthorized(problem);
			}

			throw new Exception($"Unexpected error during login: {err.Message}");
		}

		var data = serviceResult.Value!;
		
		// Fetch affiliate ID for the logged-in user
		var affiliateId = 0;
		if (int.TryParse(data.EntityId, out var parsedEntityId))
		{
			try
			{
				var affiliateResult = await businessApiService.EntityFindAsync(new()
				{
					Fields = FieldHelper<UserAffiliateDto>.Fields,
					Filter = new() { { "entityId", data.EntityId } }
				});

				if (affiliateResult.IsSuccess && affiliateResult.Value?.Entities.Count > 0)
				{
					var entity = affiliateResult.Value.Entities[0];
					affiliateId = entity.AffiliateID;
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Failed to fetch affiliate ID for user {EntityId}", data.EntityId);
			}
		}

		var credential = new Credential(
				EntityId: data.EntityId,
				Username: username,
				Password: password,
				Lid: data.Lid,
				AffiliateId: affiliateId
		);

		var encryptedToken = cookieEncryption.Encrypt(credential);
		_authCookie.Set(encryptedToken);

		if (int.TryParse(data.EntityId, out var entityId))
		{
			var signUpData = BuildUserSignUpData(includeSignupFields: false, requestedLanguage: language, clientIp: clientIp);
			await UpdateUserSignUpDataAsync(entityId, signUpData);
			await TrafficLogHelper.LogBeforeLoginVisitedPagesAsync(HttpContext, businessApiService, entityId);
			TrafficLogHelper.ClearBeforeSignupVisitedPages(HttpContext);
		}

		return Ok(new AuthResponse
		{
			Success = true,
			Code = AuthResultCodes.Success,
			EntityId = int.TryParse(data.EntityId, out var id) ? id : null,
			Username = username,
		});
	}

	private UserSignUpDataDto BuildUserSignUpData(bool includeSignupFields, string? requestedLanguage, string? clientIp)
	{
		var ip = clientIp ?? GetUserIp(HttpContext);
		var timestamp = GetCurrentTimestampUtc();
		var language = GetAcceptLanguage(requestedLanguage);
		var host = HttpContext.Request.Host.Value;

		// Use the external referrer stored in cookie (captured on first visit before signup)
		// instead of the current request's Referer header which would be an internal page
		string? referer = null;
		if (includeSignupFields)
		{
			const string ExternalRefererCookie = "8g4n5t9f4n85yg94n85gn49t5";
			if (HttpContext.Request.Cookies.TryGetValue(ExternalRefererCookie, out var storedReferer)
				&& !string.IsNullOrWhiteSpace(storedReferer))
			{
				referer = storedReferer;
				// Clear the cookie after consuming it
				HttpContext.Response.Cookies.Delete(ExternalRefererCookie);
			}
		}

		return new UserSignUpDataDto
		{
			UserCategoryId = includeSignupFields ? DefaultUserCategoryId : null,
			SignUpDate = includeSignupFields ? timestamp : null,
			LastLogin = timestamp,
			SignUpIP = includeSignupFields ? ip : null,
			LoginIp = ip,
			Language = language,
			SignUpDomain = includeSignupFields ? host : null,
			Referer = referer
		};
	}

	private async Task UpdateUserSignUpDataAsync(int entityId, UserSignUpDataDto data)
	{
		var updateResult = await businessApiService.EntityUpdateAsync(entityId, data);
		if (!updateResult.IsSuccess)
		{
			logger.LogWarning("Failed to update entity {EntityId} sign-up/login data: {Code} {Message}",
					entityId,
					updateResult.Error?.Code,
					updateResult.Error?.Message);
		}
	}

	private static string GetCurrentTimestampUtc() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

	private string? GetAcceptLanguage(string? requestedLanguage)
	{
		var normalizedRequested = NormalizeToTwoLetterLanguage(requestedLanguage);
		if (!string.IsNullOrEmpty(normalizedRequested))
		{
			return normalizedRequested;
		}

		var header = HttpContext.Request.Headers.AcceptLanguage.ToString();
		if (string.IsNullOrWhiteSpace(header)) return null;

		var firstAccepted = header.Split(',').Select(x => x.Trim()).FirstOrDefault();
		return NormalizeToTwoLetterLanguage(firstAccepted);
	}

	private static string? NormalizeToTwoLetterLanguage(string? value)
	{
		if (string.IsNullOrWhiteSpace(value)) return null;

		var primaryTag = value.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
		if (string.IsNullOrWhiteSpace(primaryTag)) return null;

		return primaryTag.Length >= 2
			? primaryTag.Substring(0, 2).ToLowerInvariant()
			: null;
	}

	private static string? GetUserIp(HttpContext context)
	{
		var cloudflareIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
		if (!string.IsNullOrWhiteSpace(cloudflareIp))
		{
			return cloudflareIp;
		}

		var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrWhiteSpace(xForwardedFor))
		{
			return xForwardedFor.Split(',').Select(x => x.Trim()).FirstOrDefault();
		}

		return context.Connection.RemoteIpAddress?.ToString();
	}

	#endregion
}
