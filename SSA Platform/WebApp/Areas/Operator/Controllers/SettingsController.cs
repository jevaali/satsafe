using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator")]
public class SettingsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}