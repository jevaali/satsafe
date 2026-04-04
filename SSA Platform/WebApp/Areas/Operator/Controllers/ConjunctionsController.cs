using System.Security.Claims;
using App.Application.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator")]
public class ConjunctionsController : Controller
{
    private readonly GetOperatorDashboardUseCase _dashboardUseCase;
    private readonly ResolveConjunctionUseCase _resolveUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public ConjunctionsController(
        GetOperatorDashboardUseCase dashboardUseCase,
        ResolveConjunctionUseCase resolveUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _dashboardUseCase        = dashboardUseCase;
        _resolveUseCase          = resolveUseCase;
        _userOperatorRepository  = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var conjunctions = await _dashboardUseCase.GetConjunctionsAsync(operatorId);
        return View(conjunctions);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var conjunction = await _dashboardUseCase.GetConjunctionByIdAsync(id, operatorId);
        if (conjunction == null) return NotFound();

        return View(conjunction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        await _resolveUseCase.ExecuteAsync(id, operatorId);
        return RedirectToAction(nameof(Index));
    }

    private async Task<Guid> GetOperatorIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return Guid.Empty;
        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return userOperator?.OperatorId ?? Guid.Empty;
    }
}