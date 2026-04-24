using Microsoft.AspNetCore.Mvc;

namespace TrustRecruitment.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
