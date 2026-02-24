namespace GoldCasino.ApiModule.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErrorController(ILogger<ErrorController> logger) : ControllerBase
{	
	[HttpGet]
	public IActionResult Get()
	{
		logger.LogCritical("An error occurred while processing the request.");
		return Problem("An error occurred while processing your request.");
	}
}
