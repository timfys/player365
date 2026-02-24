using Microsoft.AspNetCore.Mvc;

namespace SmartWinners.Controllers;

public class HomeController : Controller
{

	[HttpGet("/")]
	public IActionResult Index()
	{
		return View("/Views/HomePage.cshtml");
	}
}