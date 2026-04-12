using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Conjunctions.DTOs;
using App.Application.Features.Conjunctions.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Operator;

[ApiController]
[Route("/api/v{version:apiVersion}/operator/[controller]/[action]")]
[Authorize(Policy = "OperatorPolicy")]
public class ConjunctionsController : ControllerBase
{
    private readonly GetOperatorConjunctionsUseCase _getConjunctionsUseCase;
    private readonly GetConjunctionByIdUseCase _getConjunctionByIdUseCase;
    private readonly AcknowledgeConjunctionSideUseCase _acknowledgeUseCase;
    private readonly RefreshConjunctionStatusesUseCase _refreshUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public ConjunctionsController(
        GetOperatorConjunctionsUseCase getConjunctionsUseCase,
        GetConjunctionByIdUseCase getConjunctionByIdUseCase,
        AcknowledgeConjunctionSideUseCase acknowledgeUseCase,
        RefreshConjunctionStatusesUseCase refreshUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _getConjunctionsUseCase    = getConjunctionsUseCase;
        _getConjunctionByIdUseCase = getConjunctionByIdUseCase;
        _acknowledgeUseCase        = acknowledgeUseCase;
        _refreshUseCase            = refreshUseCase;
        _userOperatorRepository    = userOperatorRepository;
    }

    /// <summary>
    /// Returns all conjunction events for the authenticated operator. Also refreshes event statuses.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConjunctionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ConjunctionSummaryDto>>> List()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        await _refreshUseCase.ExecuteAsync(operatorId, userId);

        var conjunctions = await _getConjunctionsUseCase.ExecuteAsync(operatorId);
        return Ok(conjunctions);
    }

    /// <summary>
    /// Returns a conjunction event by ID, including which side the operator is on.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConjunctionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConjunctionDetailDto>> Details(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var conjunction = await _getConjunctionByIdUseCase.ExecuteAsync(id, operatorId);
        if (conjunction == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Conjunction event not found."
            });

        return Ok(conjunction);
    }

    /// <summary>
    /// Records the operator's acknowledgement state for their side of the conjunction.
    /// </summary>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Acknowledge(Guid id, [FromBody] EOperatorAcknowledgement state)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        try
        {
            await _acknowledgeUseCase.ExecuteAsync(id, operatorId, state);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = ex.Message
            });
        }
        catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = ex.Message
            });
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

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId)) return (Guid.Empty, Guid.Empty);
        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return (userId, userOperator?.OperatorId ?? Guid.Empty);
    }
}
