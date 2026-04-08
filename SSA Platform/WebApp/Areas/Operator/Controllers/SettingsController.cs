using System.Security.Claims;
using App.Application.DTO;
using App.Application.Interfaces.IRepositories;
using App.Application.UseCases;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public SettingsController(
        GetSettingsUseCase getSettings,
        UpdateOperatorSettingsUseCase updateOperator,
        AddHelperUseCase addHelper,
        RemoveHelperUseCase removeHelper,
        SaveNotificationsUseCase saveNotifications,
        UserManager<AppUser> userManager,
        IUserOperatorRepository userOperatorRepository)
    {
        _getSettings = getSettings;
        _updateOperator = updateOperator;
        _addHelper = addHelper;
        _removeHelper = removeHelper;
        _saveNotifications = saveNotifications;
        _userManager = userManager;
        _userOperatorRepository = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        var settings = await _getSettings.ExecuteAsync(userId, operatorId);
        var vm = BuildViewModel(settings);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOperator([Bind(Prefix = "OperatorForm")] UpdateOperatorForm form)
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
        {
            var settings = await _getSettings.ExecuteAsync(userId, operatorId);
            var vm = BuildViewModel(settings);
            vm.OperatorForm = form;
            return View("Index", vm);
        }

        try
        {
            await _updateOperator.ExecuteAsync(operatorId, new UpdateOperatorSettingsRequest
            {
                Name = form.Name,
                Country = form.Country,
                OperatorType = form.OperatorType
            });
            TempData["SuccessMessage"] = "Operator profile updated.";
            TempData["ActiveTab"] = "profile";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"] = "profile";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddHelper([Bind(Prefix = "HelperForm")] AddHelperForm form)
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter a valid email address.";
            TempData["ActiveTab"] = "team";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var targetUser = await _userManager.FindByEmailAsync(form.Email);
            if (targetUser == null)
                throw new Exception($"No user found with email '{form.Email}'.");

            await _addHelper.ExecuteAsync(operatorId, targetUser.Id);
            TempData["SuccessMessage"] = $"{form.Email} added as operator helper.";
            TempData["ActiveTab"] = "team";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"] = "team";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveHelper(Guid id)
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        try
        {
            await _removeHelper.ExecuteAsync(id, operatorId, userId);
            TempData["SuccessMessage"] = "Helper removed.";
            TempData["ActiveTab"] = "team";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"] = "team";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNotifications([Bind(Prefix = "NotificationForm")] NotificationForm form)
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty) return Forbid();

        try
        {
            await _saveNotifications.ExecuteAsync(userId, new SaveNotificationsRequest
            {
                ConjunctionThresholdKm = form.ConjunctionThresholdKm,
                AlertThresholdKm = form.AlertThresholdKm,
                EmailEnabled = form.EmailEnabled,
                SmsEnabled = form.SmsEnabled
            });
            TempData["SuccessMessage"] = "Notification preferences saved.";
            TempData["ActiveTab"] = "notifications";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            TempData["ActiveTab"] = "notifications";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<(Guid userId, Guid operatorId)> GetUserContextAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        if (userOperator == null)
            return (Guid.Empty, Guid.Empty);

        return (userId, userOperator.OperatorId);
    }

    private static SettingsViewModel BuildViewModel(SettingsDto settings) => new()
    {
        Settings = settings,
        OperatorForm = new UpdateOperatorForm
        {
            Name = settings.Operator.Name,
            Country = settings.Operator.Country,
            OperatorType = settings.Operator.OperatorType
        },
        NotificationForm = settings.NotificationSetting == null ? new NotificationForm() : new NotificationForm
        {
            ConjunctionThresholdKm = settings.NotificationSetting.ConjunctionThresholdKm,
            AlertThresholdKm       = settings.NotificationSetting.AlertThresholdKm,
            EmailEnabled           = settings.NotificationSetting.EmailEnabled,
            SmsEnabled             = settings.NotificationSetting.SmsEnabled
        }
    };
}
