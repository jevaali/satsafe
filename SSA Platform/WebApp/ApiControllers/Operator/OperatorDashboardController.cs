using System.Net;
using System.Security.Claims;
using App.Application.DTO.v1;
using App.Application.Features.Conjunctions.UseCases;
using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Dashboard.UseCases;
using App.Application.Interfaces.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.ApiControllers.Operator;

[ApiController]
[Route("/api/v{version:apiVersion}/operator/[controller]/[action]")]
[Authorize(Policy = "OperatorPolicy")]
public class DashboardController : ControllerBase
{
    private readonly GetOperatorDashboardUseCase _getDashboardUseCase;
    private readonly RunConjunctionScreeningUseCase _screeningUseCase;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public DashboardController(
        GetOperatorDashboardUseCase getDashboardUseCase,
        RunConjunctionScreeningUseCase screeningUseCase,
        IUserOperatorRepository userOperatorRepository)
    {
        _getDashboardUseCase = getDashboardUseCase;
        _screeningUseCase = screeningUseCase;
        _userOperatorRepository = userOperatorRepository;
    }

    /// <summary>
    /// Returns operator dashboard data
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(OperatorDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<OperatorDashboardDto>> GetDashboard()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);

        if (userOperator == null)
            return StatusCode((int) HttpStatusCode.Forbidden, new RestApiErrorResponse
            {
                Status = HttpStatusCode.Forbidden,
                Error  = "No operator account is linked to this user."
            });

        await _screeningUseCase.ExecuteAsync(userOperator.OperatorId, userId);

        var dashboard = await _getDashboardUseCase.ExecuteAsync(userId, userOperator.OperatorId);

        return Ok(dashboard);
    }
}