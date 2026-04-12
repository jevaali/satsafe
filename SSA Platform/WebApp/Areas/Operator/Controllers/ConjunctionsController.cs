using System.Security.Claims;
using App.Application.Features.Conjunctions.UseCases;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator,OperatorHelper")]
public class ConjunctionsController : Controller
{
    private readonly GetOperatorConjunctionsUseCase _getConjunctionsUseCase;
    private readonly GetConjunctionByIdUseCase _getConjunctionByIdUseCase;
    private readonly GetOperatorSatellitesUseCase _getSatellitesUseCase;
    private readonly ResolveConjunctionUseCase _resolveUseCase;
    private readonly AcknowledgeConjunctionSideUseCase _acknowledgeUseCase;
    private readonly RefreshConjunctionStatusesUseCase _refreshUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public ConjunctionsController(
        GetOperatorConjunctionsUseCase getConjunctionsUseCase,
        GetConjunctionByIdUseCase getConjunctionByIdUseCase,
        GetOperatorSatellitesUseCase getSatellitesUseCase,
        ResolveConjunctionUseCase resolveUseCase,
        AcknowledgeConjunctionSideUseCase acknowledgeUseCase,
        RefreshConjunctionStatusesUseCase refreshUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _getConjunctionsUseCase  = getConjunctionsUseCase;
        _getConjunctionByIdUseCase = getConjunctionByIdUseCase;
        _getSatellitesUseCase    = getSatellitesUseCase;
        _resolveUseCase          = resolveUseCase;
        _acknowledgeUseCase      = acknowledgeUseCase;
        _refreshUseCase          = refreshUseCase;
        _userOperatorRepository  = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        await _refreshUseCase.ExecuteAsync(operatorId, userId);

        var conjunctions = await _getConjunctionsUseCase.ExecuteAsync(operatorId);
        return View(conjunctions);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var conjunction = await _getConjunctionByIdUseCase.ExecuteAsync(id, operatorId);
        if (conjunction == null) return NotFound();

        var satellites = await _getSatellitesUseCase.ExecuteAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToHashSet();
        ViewBag.OperatorSide = satelliteIds.Contains(conjunction.PrimaryObjectId)   ? "Primary"
                             : satelliteIds.Contains(conjunction.SecondaryObjectId) ? "Secondary"
                             : "None";

        return View(conjunction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acknowledge(Guid id, EOperatorAcknowledgement state)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        try
        {
            await _acknowledgeUseCase.ExecuteAsync(id, operatorId, state);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return (Guid.Empty, Guid.Empty);
        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return (userId, userOperator?.OperatorId ?? Guid.Empty);
    }
}
