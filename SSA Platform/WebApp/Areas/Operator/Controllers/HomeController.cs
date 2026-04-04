using System.Security.Claims;
using App.Application.Interfaces.IRepositories;
using App.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize]
public class HomeController : Controller
{
    private readonly GetOperatorDashboardUseCase _getDashboardUseCase;
    private readonly AcknowledgeAlertUseCase _acknowledgeAlertUseCase;
    private readonly ResolveConjunctionUseCase _resolveConjunctionUseCase;
    private readonly RunConjunctionScreeningUseCase _screeningUseCase; 
    private readonly IUserOperatorRepository _userOperatorRepository;

    public HomeController(
        GetOperatorDashboardUseCase getDashboardUseCase,
        AcknowledgeAlertUseCase acknowledgeAlertUseCase,
        ResolveConjunctionUseCase resolveConjunctionUseCase,
        RunConjunctionScreeningUseCase screeningUseCase,      
        IUserOperatorRepository userOperatorRepository)
    {
        _getDashboardUseCase       = getDashboardUseCase;
        _acknowledgeAlertUseCase   = acknowledgeAlertUseCase;
        _resolveConjunctionUseCase = resolveConjunctionUseCase;
        _screeningUseCase          = screeningUseCase;  
        _userOperatorRepository    = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();
        
        await _screeningUseCase.ExecuteAsync(operatorId, userId);

        var dashboard = await _getDashboardUseCase.ExecuteAsync(userId, operatorId);
        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> Satellites()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var satellites = await _getDashboardUseCase.GetSatellitesAsync(operatorId);
        return View(satellites);
    }

    [HttpGet]
    public async Task<IActionResult> SatelliteDetails(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var satellite = await _getDashboardUseCase.GetSatelliteByIdAsync(id, operatorId);
        if (satellite == null)
            return NotFound();

        return View(satellite);
    }

    [HttpGet]
    public async Task<IActionResult> Conjunctions()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var conjunctions = await _getDashboardUseCase.GetConjunctionsAsync(operatorId);
        return View(conjunctions);
    }

    [HttpGet]
    public async Task<IActionResult> ConjunctionDetails(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var conjunction = await _getDashboardUseCase.GetConjunctionByIdAsync(id, operatorId);
        if (conjunction == null)
            return NotFound();

        return View(conjunction);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcknowledgeAlert(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        await _acknowledgeAlertUseCase.ExecuteAsync(id, operatorId);
        return RedirectToAction(nameof(Alerts));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkConjunctionResolved(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        await _resolveConjunctionUseCase.ExecuteAsync(id, operatorId);
        return RedirectToAction(nameof(Conjunctions));
    }

    [HttpGet]
    public async Task<IActionResult> Alerts()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var alerts = await _getDashboardUseCase.GetAlertsAsync(operatorId);
        return View(alerts);
    }

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (Guid.Empty, Guid.Empty);

        if (!User.IsInRole("Operator"))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        if (userOperator == null)
            return (Guid.Empty, Guid.Empty);

        return (userId, userOperator.OperatorId);
    }
}
