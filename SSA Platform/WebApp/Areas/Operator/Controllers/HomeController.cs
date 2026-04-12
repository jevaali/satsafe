using System.Security.Claims;
using App.Application.Features.Conjunctions.UseCases;
using App.Application.Features.Dashboard.UseCases;
using App.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator,OperatorHelper")]
public class HomeController : Controller
{
    private readonly GetOperatorDashboardUseCase _getDashboardUseCase;
    private readonly RunConjunctionScreeningUseCase _screeningUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public HomeController(
        GetOperatorDashboardUseCase getDashboardUseCase,
        RunConjunctionScreeningUseCase screeningUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _getDashboardUseCase     = getDashboardUseCase;
        _screeningUseCase          = screeningUseCase;
        _userOperatorRepository    = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        
        if (userOperator == null)
            return Forbid();

        await _screeningUseCase.ExecuteAsync(userOperator.OperatorId, userId);

        var dashboard = await _getDashboardUseCase.ExecuteAsync(userId, userOperator.OperatorId);
        return View(dashboard);
    }
}
