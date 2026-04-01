using App.Application.DTO;
using App.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class SatelliteController : Controller
{
    private readonly AddSatelliteUseCase _useCase;

    public SatelliteController(AddSatelliteUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddSatelliteRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var id = await _useCase.Execute(request);

        return RedirectToAction("Details", new { id });
    }
}