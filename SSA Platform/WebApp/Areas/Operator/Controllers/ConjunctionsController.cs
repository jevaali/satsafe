using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator")]
public class ConjunctionsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}