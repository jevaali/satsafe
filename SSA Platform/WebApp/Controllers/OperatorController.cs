using App.Application.DTO;
using App.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class OperatorController : Controller
{
    private readonly AddOperatorUseCase _useCase;

    public OperatorController(AddOperatorUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddOperatorRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var id = await _useCase.Execute(request);

        return RedirectToAction("Index", "Home");
    }
}