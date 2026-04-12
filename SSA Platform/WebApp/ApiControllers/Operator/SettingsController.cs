using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Dashboard.UseCases;
using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Notifications.UseCases;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Operators.UseCases;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// Returns the operator's settings including profile, team, and notification preferences.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SettingsDto>> Index()
    {
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        return Ok(await _getSettings.ExecuteAsync(userId, operatorId));
    }

    /// <summary>
    /// Returns the operator's action logs, optionally filtered by a specific team member.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OperatorActionLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<OperatorActionLogDto>>> ActionLogs([FromQuery] Guid? filterUserId = null)
    {
        var (_, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        return Ok(await _getActionLogs.ExecuteAsync(operatorId, filterUserId));
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

        var (_, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
    }

    /// <summary>
    /// Adds a user as an operator helper by email. Requires Operator role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddHelper([FromBody] AddHelperRequest request)
    {
        if (!User.IsInRole("Operator"))
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "Operator role required."
            });

        var (_, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        try
        {
            await _addHelper.ExecuteAsync(operatorId, request);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
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

        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
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
        var (userId, operatorId) = await _getUserContext.ExecuteAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            return BadRequest(new RestApiErrorResponse { Status = HttpStatusCode.BadRequest, Error = ex.Message });
        }
    }
}
