using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Dashboard.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public class MainListController : Controller
{
    private readonly GetMainListUseCase _getMainListUseCase;

    public MainListController(GetMainListUseCase getMainListUseCase)
    {
        _getMainListUseCase = getMainListUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        MainListDTO model = await _getMainListUseCase.ExecuteAsync();
        return View(model);
    }
}