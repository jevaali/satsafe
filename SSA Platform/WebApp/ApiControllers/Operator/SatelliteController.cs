using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Operator;

[ApiController]
[Route("/api/v{version:apiVersion}/operator/[controller]/[action]")]
[Authorize(Policy = "OperatorPolicy")]
public class SatelliteController : ControllerBase
{
    private readonly AddSatelliteUseCase _addSatelliteUseCase;
    private readonly UpdateSatelliteUseCase _updateSatelliteUseCase;
    private readonly DeleteSatelliteUseCase _deleteSatelliteUseCase;
    private readonly GetSatelliteByIdUseCase _getSatelliteByIdUseCase;
    private readonly GetOperatorSatellitesUseCase _getOperatorSatellitesUseCase;
    private readonly CalculateSatelliteRiskUseCase _calculateRiskUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public SatelliteController(
        AddSatelliteUseCase addSatelliteUseCase,
        UpdateSatelliteUseCase updateSatelliteUseCase,
        DeleteSatelliteUseCase deleteSatelliteUseCase,
        GetSatelliteByIdUseCase getSatelliteByIdUseCase,
        GetOperatorSatellitesUseCase getOperatorSatellitesUseCase,
        CalculateSatelliteRiskUseCase calculateRiskUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _addSatelliteUseCase          = addSatelliteUseCase;
        _updateSatelliteUseCase       = updateSatelliteUseCase;
        _deleteSatelliteUseCase       = deleteSatelliteUseCase;
        _getSatelliteByIdUseCase      = getSatelliteByIdUseCase;
        _getOperatorSatellitesUseCase = getOperatorSatellitesUseCase;
        _calculateRiskUseCase         = calculateRiskUseCase;
        _userOperatorRepository       = userOperatorRepository;
    }

    /// <summary>
    /// Returns all satellites belonging to the authenticated operator.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SatelliteSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SatelliteSummaryDto>>> List()
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var satellites = await _getOperatorSatellitesUseCase.ExecuteAsync(operatorId);

        return Ok(satellites);
    }

    /// <summary>
    /// Returns a satellite by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SatelliteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SatelliteDetailDto>> Details(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);

        if (satellite == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Satellite not found."
            });

        return Ok(satellite);
    }

    /// <summary>
    /// Adds a new satellite. Requires Operator role.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "OperatorOnlyPolicy")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> Create([FromBody] AddSatelliteRequest request)
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
            request.OperatorId = operatorId;
            var id = await _addSatelliteUseCase.Execute(request);
            return CreatedAtAction(nameof(Details), new { id }, id);
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
    /// Updates an existing satellite. Requires Operator role.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OperatorOnlyPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSatelliteRequest request)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Satellite not found."
            });

        try
        {
            request.SatelliteId = id;
            request.OperatorId  = operatorId;
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "operator";
            await _updateSatelliteUseCase.Execute(id, request, username);
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
    /// Soft-deletes a satellite. Requires Operator role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "OperatorOnlyPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Satellite not found."
            });

        try
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "operator";
            await _deleteSatelliteUseCase.Execute(id, operatorId, username);
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
    /// Recalculates the risk score for a satellite. Requires Operator role.
    /// </summary>
    [HttpPost("{id:guid}")]
    [Authorize(Policy = "OperatorOnlyPolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegenerateRisk(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error  = "Satellite not found."
            });

        try
        {
            await _calculateRiskUseCase.CalculateForSatelliteAsync(id);
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

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        return userOperator == null
            ? (Guid.Empty, Guid.Empty)
            : (userId, userOperator.OperatorId);
    }
}
