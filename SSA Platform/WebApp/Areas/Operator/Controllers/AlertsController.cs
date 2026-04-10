using System.Security.Claims;
using App.Application.UseCases;
using App.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator")]
public class AlertsController : Controller
{
    private readonly GetOperatorDashboardUseCase _dashboardUseCase;
    private readonly AcknowledgeAlertUseCase _acknowledgeUseCase;
    private readonly UpdateAlertMessageUseCase _updateMessageUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public AlertsController(
        GetOperatorDashboardUseCase dashboardUseCase,
        AcknowledgeAlertUseCase acknowledgeUseCase,
        UpdateAlertMessageUseCase updateMessageUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _dashboardUseCase       = dashboardUseCase;
        _acknowledgeUseCase     = acknowledgeUseCase;
        _updateMessageUseCase   = updateMessageUseCase;
        _userOperatorRepository = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var alerts = await _dashboardUseCase.GetAlertsAsync(operatorId);
        return View(alerts);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var alert = await _dashboardUseCase.GetAlertByIdAsync(id, operatorId);
        if (alert == null) return NotFound();

        return View(alert);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        await _acknowledgeUseCase.ExecuteAsync(id, operatorId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> EditMessage(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var alert = await _dashboardUseCase.GetAlertByIdAsync(id, operatorId);
        if (alert == null) return NotFound();

        ViewBag.CurrentMessage = alert.Message?.ToString() ?? string.Empty;
        return View(alert);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMessage(Guid id, string message)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty) return Forbid();

        if (string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError(nameof(message), "Message cannot be empty.");
            var alert = await _dashboardUseCase.GetAlertByIdAsync(id, operatorId);
            if (alert == null) return NotFound();
            ViewBag.CurrentMessage = message;
            return View(alert);
        }

        await _updateMessageUseCase.ExecuteAsync(id, operatorId, message.Trim());
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Guid> GetOperatorIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return Guid.Empty;
        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return userOperator?.OperatorId ?? Guid.Empty;
    }
}