using System.Diagnostics;
using App.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly GetHomeStatsUseCase _getHomeStatsUseCase;

    public HomeController(GetHomeStatsUseCase getHomeStatsUseCase)
    {
        _getHomeStatsUseCase = getHomeStatsUseCase;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Operator"))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Operator" });
        }
        
        var stats = await _getHomeStatsUseCase.ExecuteAsync();
        return View(stats);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}