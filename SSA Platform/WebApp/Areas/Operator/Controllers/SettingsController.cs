using System.Security.Claims;
using App.Application.Features.Dashboard.UseCases;
using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Notifications.UseCases;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Operators.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Areas.Operator.Models;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator")]
public class SettingsController : Controller
{
    private readonly GetSettingsUseCase _getSettings;
    private readonly UpdateOperatorSettingsUseCase _updateOperator;
    private readonly AddHelperUseCase _addHelper;
    private readonly RemoveHelperUseCase _removeHelper;
    private readonly SaveNotificationsUseCase _saveNotifications;
    private readonly GetOperatorActionLogsUseCase _getActionLogs;
    private readonly GetUserContextUseCase _getUserContext;

    public SettingsController(
        GetSettingsUseCase getSettings,
        UpdateOperatorSettingsUseCase updateOperator,
        AddHelperUseCase addHelper,
        RemoveHelperUseCase removeHelper,
        SaveNotificationsUseCase saveNotifications,
        GetOperatorActionLogsUseCase getActionLogs,
        GetUserContextUseCase getUserContext)
    {
        _getSettings       = getSettings;
        _updateOperator    = updateOperator;
        _addHelper         = addHelper;
        _removeHelper      = removeHelper;
        _saveNotifications = saveNotifications;
        _getActionLogs     = getActionLogs;
        _getUserContext    = getUserContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? filterUserId)
    {
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        var settings = await _getSettings.ExecuteAsync(userId, operatorId);
        var logs     = await _getActionLogs.ExecuteAsync(operatorId, filterUserId);

        var vm = BuildViewModel(settings);
        vm.ActionLogs            = logs;
        vm.ActivityFilterUserId  = filterUserId;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOperator([Bind(Prefix = "OperatorForm")] UpdateOperatorSettingsRequest request)
    {
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
        {
            var settings = await _getSettings.ExecuteAsync(userId, operatorId);
            var vm = BuildViewModel(settings);
            vm.OperatorForm = request;
            return View("Index", vm);
        }

        try
        {
            await _updateOperator.ExecuteAsync(operatorId, request);
            TempData["SuccessMessage"] = "Operator profile updated.";
            TempData["ActiveTab"]      = "profile";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"]    = "profile";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddHelper([Bind(Prefix = "HelperForm")] AddHelperRequest request)
    {
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter a valid email address.";
            TempData["ActiveTab"]    = "team";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _addHelper.ExecuteAsync(operatorId, request);
            TempData["SuccessMessage"] = $"{request.Email} added as operator helper.";
            TempData["ActiveTab"]      = "team";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"]    = "team";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveHelper(Guid id)
    {
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        try
        {
            await _removeHelper.ExecuteAsync(id, operatorId, userId);
            TempData["SuccessMessage"] = "Helper removed.";
            TempData["ActiveTab"]      = "team";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"]    = "team";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNotifications([Bind(Prefix = "NotificationForm")] SaveNotificationsRequest request)
    {
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        try
        {
            await _saveNotifications.ExecuteAsync(userId, request);
            TempData["SuccessMessage"] = "Notification preferences saved.";
            TempData["ActiveTab"]      = "notifications";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"]    = "notifications";
        }

        return RedirectToAction(nameof(Index));
    }

    private static SettingsViewModel BuildViewModel(SettingsDto settings) => new()
    {
        Settings     = settings,
        OperatorForm = new UpdateOperatorSettingsRequest
        {
            Name         = settings.Operator.Name,
            Country      = settings.Operator.CountryCode,
            OperatorType = settings.Operator.OperatorType
        },
        NotificationForm = settings.NotificationSetting == null ? new SaveNotificationsRequest() : new SaveNotificationsRequest
        {
            ConjunctionThresholdKm = settings.NotificationSetting.ConjunctionThresholdKm,
            AlertThresholdKm       = settings.NotificationSetting.AlertThresholdKm,
            EmailEnabled           = settings.NotificationSetting.EmailEnabled,
            SmsEnabled             = settings.NotificationSetting.SmsEnabled
        }
    };
}
