using System;
using System.Threading;
using GoldCasino.ApiModule.Common.Exceptions; // Import your exceptions
using GoldCasino.ApiModule.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SmartWinners.Controllers;

[Route("Error")]
public class ErrorsHandlerController(
    ILogger<ErrorsHandlerController> logger, 
    IAuthCookieService authCookie) : ControllerBase
{
    [Route("Handler")]
    public IActionResult Index()
    {
        // // 1. Prevent infinite loops
        // if (HttpContext.Request.Path.StartsWithSegments("/Error", StringComparison.OrdinalIgnoreCase))
        //     return StatusCode(500, "Recursive error detected");

        var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandler is null)
            return NotFound();

        var exception = exceptionHandler.Error;
        var path = exceptionHandler.Path;

        // 2. Setup Language (Note: On the error path, culture might reset, so be careful here)
        var langIso = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
        langIso = langIso.Equals("en", StringComparison.OrdinalIgnoreCase) ? "" : $"/{langIso}";

        // 3. Handle Authentication Exceptions (Redirect to Login)
        if (exception is AuthenticationServiceException)
        {
            // You might want to clear the cookie here too: authCookie.Delete();
            // Note: You can't inject IAuthCookieService easily here if the request is already broken, 
            // but the Redirect usually suffices.
            
            logger.LogWarning("Authentication error in view: {Message}", exception.Message);
						authCookie.Delete();
            return Redirect($"{langIso}/sign-in?r={path}"); // Redirect back to original path after login
        }

        // 4. Handle Upstream/Database Exceptions (Log and show 500)
        logger.LogError(exception, "Unhandled exception occurred on path {Path}", path);

        // Optional: Add header (Be careful: this might fail if headers were already sent)
        if (!Response.HasStarted)
        {
            Response.Headers.Append("Error", exception.Message);
        }

        return Redirect($"{langIso}/500Error");
    }
}

// using Microsoft.AspNetCore.Diagnostics;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Threading;

// namespace SmartWinners.Controllers;

// [Route("Error")]
// public class ErrorsHandlerController(ILogger<ErrorsHandlerController> logger) : ControllerBase
// {
// 	[Route("Handler")]
// 	public IActionResult Index()
// 	{
// 		if (HttpContext.Request.Path.StartsWithSegments("/Error", StringComparison.OrdinalIgnoreCase))
// 			return StatusCode(500, "Recursive error detected");
						
// 		var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
// 		if (exceptionHandler is null)
// 			return NotFound();

// 		var exception = exceptionHandler.Error;
// 		var path = exceptionHandler.Path;

// 		// Log the error details using Serilog
// 		logger.LogError(exception, "Unhandled exception occurred on path {Path}", path);

// 		// Optionally, you can add any additional context here
// 		// For example, you might add user info if available

// 		var langIso = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
// 		langIso = langIso.Equals("en") ? "" : $"/{langIso}";

// 		// Optionally add the error message to the response header
// 		Response.Headers.Append("Error", exception.Message);

// 		return Redirect($"{langIso}/500Error");
// 	}
// }	

