using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Dashboard.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers;

[ApiController]
[Route("/api/v{version:apiVersion}/[controller]/[action]")]
public class MainListController : ControllerBase
{
    private readonly GetMainListUseCase _getMainListUseCase;

    public MainListController(GetMainListUseCase getMainListUseCase)
    {
        _getMainListUseCase = getMainListUseCase;
    }

    /// <summary>
    /// Returns the main list dashboard data.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(MainListDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<MainListDTO>> Index()
    {
        var model = await _getMainListUseCase.ExecuteAsync();
        return Ok(model);
    }
}