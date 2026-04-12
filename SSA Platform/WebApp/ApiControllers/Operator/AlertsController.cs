using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Alerts.UseCases;
using App.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Operator;

[ApiController]
[Route("/api/v{version:apiVersion}/operator/[controller]/[action]")]
[Authorize(Policy = "OperatorPolicy")]
public class AlertsController : ControllerBase
{
    private readonly GetOperatorAlertsUseCase _getAlertsUseCase;
    private readonly GetAlertByIdUseCase _getAlertByIdUseCase;
    private readonly AcknowledgeAlertUseCase _acknowledgeUseCase;
    private readonly UpdateAlertMessageUseCase _updateMessageUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public AlertsController(
        GetOperatorAlertsUseCase getAlertsUseCase,
        GetAlertByIdUseCase getAlertByIdUseCase,
        AcknowledgeAlertUseCase acknowledgeUseCase,
        UpdateAlertMessageUseCase updateMessageUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _getAlertsUseCase       = getAlertsUseCase;
        _getAlertByIdUseCase    = getAlertByIdUseCase;
        _acknowledgeUseCase     = acknowledgeUseCase;
        _updateMessageUseCase   = updateMessageUseCase;
        _userOperatorRepository = userOperatorRepository;
    }

    /// <summary>
    /// Returns all alerts for the authenticated operator.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlertSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AlertSummaryDto>>> List()
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        return Ok(await _getAlertsUseCase.ExecuteAsync(operatorId));
    }

    /// <summary>
    /// Returns an alert by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlertDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertDetailsDto>> Details(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var alert = await _getAlertByIdUseCase.ExecuteAsync(id, operatorId);
        if (alert == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Alert not found."
            });

        return Ok(alert);
    }

    /// <summary>
    /// Acknowledges an alert.
    /// </summary>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var alert = await _getAlertByIdUseCase.ExecuteAsync(id, operatorId);
        if (alert == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Alert not found."
            });

        await _acknowledgeUseCase.ExecuteAsync(id, operatorId);
        return NoContent();
    }

    /// <summary>
    /// Updates the custom message on an alert.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMessage(Guid id, [FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error  = "Message cannot be empty."
            });

        var operatorId = await GetOperatorIdAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var alert = await _getAlertByIdUseCase.ExecuteAsync(id, operatorId);
        if (alert == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Alert not found."
            });

        await _updateMessageUseCase.ExecuteAsync(id, operatorId, message.Trim());
        return NoContent();
    }

    private async Task<Guid> GetOperatorIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return Guid.Empty;
        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return userOperator?.OperatorId ?? Guid.Empty;
    }
}
