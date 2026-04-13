using System.Security.Claims;
using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces.IRepositories;
using App.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator,OperatorHelper")]
public class SatelliteController : Controller
{
    private readonly AddSatelliteUseCase _addSatelliteUseCase;
    private readonly UpdateSatelliteUseCase _updateSatelliteUseCase;
    private readonly DeleteSatelliteUseCase _deleteSatelliteUseCase;
    private readonly GetSatelliteByIdUseCase _getSatelliteByIdUseCase;
    private readonly GetOperatorSatellitesUseCase _getOperatorSatellitesUseCase;
    private readonly CalculateSatelliteRiskUseCase _calculateRiskUseCase;
    private readonly ImportTleFromCelesTrakUseCase _importTle;
    private readonly AddTleManuallyUseCase _addTleManually;
    private readonly CelesTrakOptions _celestrakOptions;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public SatelliteController(
        AddSatelliteUseCase addSatelliteUseCase,
        UpdateSatelliteUseCase updateSatelliteUseCase,
        DeleteSatelliteUseCase deleteSatelliteUseCase,
        GetSatelliteByIdUseCase getSatelliteByIdUseCase,
        GetOperatorSatellitesUseCase getOperatorSatellitesUseCase,
        CalculateSatelliteRiskUseCase calculateRiskUseCase,
        ImportTleFromCelesTrakUseCase importTle,
        AddTleManuallyUseCase addTleManually,
        IOptions<CelesTrakOptions> celestrakOptions,
        IUserOperatorRepository userOperatorRepository)
    {
        _addSatelliteUseCase          = addSatelliteUseCase;
        _updateSatelliteUseCase       = updateSatelliteUseCase;
        _deleteSatelliteUseCase       = deleteSatelliteUseCase;
        _getSatelliteByIdUseCase      = getSatelliteByIdUseCase;
        _getOperatorSatellitesUseCase = getOperatorSatellitesUseCase;
        _calculateRiskUseCase         = calculateRiskUseCase;
        _importTle                    = importTle;
        _addTleManually               = addTleManually;
        _celestrakOptions             = celestrakOptions.Value;
        _userOperatorRepository       = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var satellites = await _getOperatorSatellitesUseCase.ExecuteAsync(operatorId);
        return View(satellites);
    }

    [HttpGet]
    [Authorize(Roles = "Operator")]
    public IActionResult Create()
    {
        return View(new AddSatelliteRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Create([FromForm] AddSatelliteRequest request)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
            return View(request);

        try
        {
            request.OperatorId = operatorId;
            await _addSatelliteUseCase.Execute(request);
            TempData["SuccessMessage"] = "Satellite added successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound();

        var request = new UpdateSatelliteRequest
        {
            SatelliteId       = satellite.Id,
            Name              = satellite.Name,
            LaunchDate        = satellite.LaunchDate,
            OperatorId        = operatorId,
            NoradId           = satellite.NoradId == 0 ? null : satellite.NoradId,
            SemiMajorAxis     = satellite.Orbit?.SemiMajorAxis,
            Eccentricity      = satellite.Orbit?.Eccentricity,
            Inclination       = satellite.Orbit?.Inclination,
            RAAN              = satellite.Orbit?.RAAN,
            ArgumentOfPerigee = satellite.Orbit?.ArgumentOfPerigee,
            MeanAnomaly       = satellite.Orbit?.MeanAnomaly
        };

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Edit([FromForm] UpdateSatelliteRequest request)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
            return View(request);

        try
        {
            request.OperatorId = operatorId;
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "operator";
            await _updateSatelliteUseCase.Execute(request.SatelliteId, request, username);
            TempData["SuccessMessage"] = "Satellite updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound();

        return View(satellite);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        try
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "operator";
            await _deleteSatelliteUseCase.Execute(id, operatorId, username);
            TempData["SuccessMessage"] = "Satellite deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound();

        ViewData["CelesTrakFetchEnabled"] = _celestrakOptions.FetchEnabled;
        return View(satellite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> RegenerateRisk(Guid id)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        var satellite = await _getSatelliteByIdUseCase.ExecuteAsync(id, operatorId);
        if (satellite == null)
            return NotFound();

        try
        {
            await _calculateRiskUseCase.CalculateForSatelliteAsync(id);
            TempData["SuccessMessage"] = "Risk score recalculated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> FetchTle(Guid id)
    {
        if (!_celestrakOptions.FetchEnabled)
        {
            TempData["ErrorMessage"] = "CelesTrak fetching is currently disabled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        try
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "operator";
            await _importTle.ExecuteAsync(id, username, operatorId);
            TempData["SuccessMessage"] = "TLE fetched from CelesTrak successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Operator,OperatorHelper")]
    public async Task<IActionResult> AddTleManually(Guid id, [FromForm] AddTleManuallyRequest request)
    {
        var (_, operatorId) = await GetOperatorContextAsync();
        if (operatorId == Guid.Empty) return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? "operator";
            await _addTleManually.ExecuteAsync(id, request.RawLine1, request.RawLine2, username, operatorId);
            TempData["SuccessMessage"] = "TLE record added successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
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
