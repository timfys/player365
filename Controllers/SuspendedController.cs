using Microsoft.AspNetCore.Mvc;

namespace SmartWinners.Controllers;

[Route("")]
public class SuspendedController : Controller
{
    [HttpGet("/suspended")]
    [HttpGet("{langIso}/suspended")]
    public IActionResult Index([FromRoute(Name = "langIso")] string langIso)
    {
        return View("/Views/Suspended.cshtml");
    }
}