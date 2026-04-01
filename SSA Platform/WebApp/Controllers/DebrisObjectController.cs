using App.Application.DTO;
using App.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers;

public class DebrisObjectController : Controller
{
    private readonly AddDebrisObjectUseCase _useCase;
    private readonly ILogger<DebrisObjectController> _logger;

    public DebrisObjectController(AddDebrisObjectUseCase useCase, ILogger<DebrisObjectController> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddDebrisObjectRequest request)
    {
        _logger.LogInformation("Create DebrisObject called - Size: {Size}, Risk: {Risk}", 
            request.SizeEstimate, request.RiskCategory);
        _logger.LogInformation("Orbit params - SMA: {SMA}, Ecc: {Ecc}, Inc: {Inc}",
            request.SemiMajorAxis, request.Eccentricity, request.Inclination);
        
        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                if (errors.Any())
                {
                    _logger.LogWarning("Validation error on {Key}: {Errors}", key, string.Join(", ", errors.Select(e => e.ErrorMessage)));
                }
            }
            return View(request);
        }

        try
        {
            var id = await _useCase.Execute(request);
            _logger.LogInformation("DebrisObject created with ID: {Id}", id);
            return RedirectToAction("Details", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in Create: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }
}
