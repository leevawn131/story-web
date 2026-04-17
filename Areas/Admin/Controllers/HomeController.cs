using Microsoft.AspNetCore.Mvc;
using story_web.Filters;
using story_web.Models;

namespace story_web.Areas.Admin.Controllers;

[Area("Admin")]
[Auth(UserRoles.Admin)]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
