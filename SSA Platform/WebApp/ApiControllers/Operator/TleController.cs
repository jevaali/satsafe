using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApp.ApiControllers.Operator;

[ApiController]
[Route("/api/v{version:apiVersion}/operator/[controller]/[action]")]
[Authorize(Policy = "OperatorPolicy")]
public class TleController : ControllerBase
{
    private readonly ImportTleFromCelesTrakUseCase _importTle;
    private readonly AddTleManuallyUseCase _addTleManually;
    private readonly GetSatelliteByIdUseCase _getSatelliteByIdUseCase;
    private readonly CelesTrakOptions _celestrakOptions;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public TleController(
        ImportTleFromCelesTrakUseCase importTle,
        AddTleManuallyUseCase addTleManually,
        GetSatelliteByIdUseCase getSatelliteByIdUseCase,
        IOptions<CelesTrakOptions> celestrakOptions,
        IUserOperatorRepository userOperatorRepository)
    {
        _importTle               = importTle;
        _addTleManually          = addTleManually;
        _getSatelliteByIdUseCase = getSatelliteByIdUseCase;
        _celestrakOptions        = celestrakOptions.Value;
        _userOperatorRepository  = userOperatorRepository;
    }

    /// <summary>
    /// Manually adds a TLE record to a satellite. Requires Operator or OperatorHelper role.
    /// </summary>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTleManually(Guid id, [FromBody] AddTleManuallyRequest request)
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
            await _addTleManually.ExecuteAsync(id, request.RawLine1, request.RawLine2, username, operatorId);
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