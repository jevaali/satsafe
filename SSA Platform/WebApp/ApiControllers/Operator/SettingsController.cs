using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Dashboard.UseCases;
using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Notifications.UseCases;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Operators.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Operator;

[ApiController]
[Route("/api/v{version:apiVersion}/operator/[controller]/[action]")]
[Authorize(Policy = "OperatorPolicy")]
public class SettingsController : ControllerBase
{
    private readonly GetSettingsUseCase _getSettings;
    private readonly UpdateOperatorSettingsUseCase _updateOperator;
    private readonly AddHelperUseCase _addHelper;
    private readonly RemoveHelperUseCase _removeHelper;
    private readonly SaveNotificationsUseCase _saveNotifications;
    private readonly GetOperatorActionLogsUseCase _getActionLogs;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public SettingsController(
        GetSettingsUseCase getSettings,
        UpdateOperatorSettingsUseCase updateOperator,
        AddHelperUseCase addHelper,
        RemoveHelperUseCase removeHelper,
        SaveNotificationsUseCase saveNotifications,
        GetOperatorActionLogsUseCase getActionLogs,
        UserManager<AppUser> userManager,
        IUserOperatorRepository userOperatorRepository)
    {
        _getSettings            = getSettings;
        _updateOperator         = updateOperator;
        _addHelper              = addHelper;
        _removeHelper           = removeHelper;
        _saveNotifications      = saveNotifications;
        _getActionLogs          = getActionLogs;
        _userManager            = userManager;
        _userOperatorRepository = userOperatorRepository;
    }

    /// <summary>
    /// Returns the operator's settings including profile, team, and notification preferences.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsDto>> Index()
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var settings = await _getSettings.ExecuteAsync(userId, operatorId);
        return Ok(settings);
    }

    /// <summary>
    /// Returns the operator's action logs, optionally filtered by a specific team member.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OperatorActionLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OperatorActionLogDto>>> ActionLogs([FromQuery] Guid? filterUserId = null)
    {
        var (_, operatorId) = await GetUserContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var logs = await _getActionLogs.ExecuteAsync(operatorId, filterUserId);
        return Ok(logs);
    }

    /// <summary>
    /// Updates the operator's profile. Requires Operator role.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOperator([FromBody] UpdateOperatorSettingsRequest request)
    {
        if (!User.IsInRole("Operator"))
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "Operator role required."
            });

        var (_, operatorId) = await GetUserContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        try
        {
            await _updateOperator.ExecuteAsync(operatorId, request);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error  = ex.Message
            });
        }
    }

    /// <summary>
    /// Adds a user as an operator helper by email. Requires Operator role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddHelper([FromBody] string email)
    {
        if (!User.IsInRole("Operator"))
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "Operator role required."
            });

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error  = "Email cannot be empty."
            });

        var (_, operatorId) = await GetUserContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        try
        {
            var targetUser = await _userManager.FindByEmailAsync(email);
            if (targetUser == null)
                return BadRequest(new RestApiErrorResponse
                {
                    Status = HttpStatusCode.BadRequest,
                    Error  = $"No user found with email '{email}'."
                });

            await _addHelper.ExecuteAsync(operatorId, targetUser.Id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error  = ex.Message
            });
        }
    }

    /// <summary>
    /// Removes an operator helper. Requires Operator role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveHelper(Guid id)
    {
        if (!User.IsInRole("Operator"))
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "Operator role required."
            });

        var (userId, operatorId) = await GetUserContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        try
        {
            await _removeHelper.ExecuteAsync(id, operatorId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error  = ex.Message
            });
        }
    }

    /// <summary>
    /// Saves the user's notification preferences.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SaveNotifications([FromBody] SaveNotificationsRequest request)
    {
        var (userId, operatorId) = await GetUserContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        try
        {
            await _saveNotifications.ExecuteAsync(userId, request);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error  = ex.Message
            });
        }
    }

    private async Task<(Guid userId, Guid operatorId)> GetUserContextAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return userOperator == null
            ? (Guid.Empty, Guid.Empty)
            : (userId, userOperator.OperatorId);
    }
}
