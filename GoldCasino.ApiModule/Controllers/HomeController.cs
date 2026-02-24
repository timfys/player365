namespace GoldCasino.ApiModule.Controllers;

public class HomeController : Controller
{
  [HttpGet]
  public IActionResult Index()
  {
    return Content("Home Index - culture: " + System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
  }

  // Conventional route mapping handles /{culture}/hi (see Program.cs route 'home_short')
  public IActionResult Hi()
  {
    return Content("Home Hi - culture: " + System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
  }
}
