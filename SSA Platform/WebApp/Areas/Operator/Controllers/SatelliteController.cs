using System.Security.Claims;
using App.Application.DTO;
using App.Application.Interfaces.IRepositories;
using App.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Areas.Operator.Controllers;

[Area("Operator")]
[Authorize(Roles = "Operator")]
public class SatelliteController : Controller
{
    private readonly AddSatelliteUseCase _addSatelliteUseCase;
    private readonly UpdateSatelliteUseCase _updateSatelliteUseCase;
    private readonly DeleteSatelliteUseCase _deleteSatelliteUseCase;
    private readonly ISatelliteRepository _satelliteRepository;
    private readonly IUserOperatorRepository _userOperatorRepository;

    public SatelliteController(
        AddSatelliteUseCase addSatelliteUseCase,
        UpdateSatelliteUseCase updateSatelliteUseCase,
        DeleteSatelliteUseCase deleteSatelliteUseCase,
        ISatelliteRepository satelliteRepository,
        IUserOperatorRepository userOperatorRepository)
    {
        _addSatelliteUseCase = addSatelliteUseCase;
        _updateSatelliteUseCase = updateSatelliteUseCase;
        _deleteSatelliteUseCase = deleteSatelliteUseCase;
        _satelliteRepository = satelliteRepository;
        _userOperatorRepository = userOperatorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var satellites = await _satelliteRepository.GetByOperatorIdAsync(operatorId);
        return View(satellites);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AddSatelliteRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] AddSatelliteRequest request)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

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
    public async Task<IActionResult> Edit(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var satellite = await _satelliteRepository.GetByIdAsync(id);
        if (satellite == null || satellite.DeletedAt != null)
            return NotFound();

        if (satellite.OperatorId != operatorId)
            return Forbid();

        var request = new UpdateSatelliteRequest
        {
            SatelliteId = satellite.Id,
            Name = satellite.Name,
            LaunchDate = satellite.LaunchDate,
            OperatorId = satellite.OperatorId,
            SemiMajorAxis = satellite.Orbit?.Parameters?.SemiMajorAxis ?? 0,
            Eccentricity = satellite.Orbit?.Parameters?.Eccentricity ?? 0,
            Inclination = satellite.Orbit?.Parameters?.Inclination ?? 0,
            RAAN = satellite.Orbit?.Parameters?.RAAN ?? 0,
            ArgumentOfPerigee = satellite.Orbit?.Parameters?.ArgumentOfPerigee ?? 0,
            MeanAnomaly = satellite.Orbit?.Parameters?.MeanAnomaly ?? 0
        };

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] UpdateSatelliteRequest request)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        if (!ModelState.IsValid)
            return View(request);

        try
        {
            request.OperatorId = operatorId;
            await _updateSatelliteUseCase.Execute(request.SatelliteId, request);
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
    public async Task<IActionResult> Delete(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var satellite = await _satelliteRepository.GetByIdAsync(id);
        if (satellite == null || satellite.DeletedAt != null)
            return NotFound();

        if (satellite.OperatorId != operatorId)
            return Forbid();

        return View(satellite);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

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
        var (userId, operatorId) = await GetOperatorContextAsync();
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            return Forbid();

        var satellite = await _satelliteRepository.GetByIdAsync(id);
        if (satellite == null || satellite.DeletedAt != null)
            return NotFound();

        if (satellite.OperatorId != operatorId)
            return Forbid();

        return View(satellite);
    }

    private async Task<(Guid userId, Guid operatorId)> GetOperatorContextAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return (Guid.Empty, Guid.Empty);

        var userOperator = await _userOperatorRepository.GetByUserIdAsync(userId);
        if (userOperator == null)
            return (Guid.Empty, Guid.Empty);

        return (userId, userOperator.OperatorId);
    }
}