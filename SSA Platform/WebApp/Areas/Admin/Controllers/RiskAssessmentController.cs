using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RiskAssessmentController : Controller
{
    private readonly AppDbContext _context;

    public RiskAssessmentController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/RiskAssessment
    public async Task<IActionResult> Index()
    {
        var assessments = await _context.RiskAssessments
            .Include(r => r.Satellite)
            .OrderByDescending(r => r.RiskScore)
            .ToListAsync();

        var vm = new RiskAssessmentIndexViewModel
        {
            TotalCount    = assessments.Count,
            HighRiskCount = assessments.Count(r => r.RiskScore > 0.7),
            Assessments   = assessments.Select(r => new RiskAssessmentListItemViewModel
            {
                Id            = r.Id,
                RiskScore     = r.RiskScore,
                ModelVersion  = r.ModelVersion,
                CalculatedAt  = r.CalculatedAt,
                SatelliteId   = r.SatelliteId,
                SatelliteName = r.Satellite.Name.ToString()
            })
        };
        return View(vm);
    }

    // GET: Admin/RiskAssessment/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var r = await _context.RiskAssessments
            .Include(r => r.Satellite)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (r == null) return NotFound();

        var vm = new RiskAssessmentDetailViewModel
        {
            Id            = r.Id,
            RiskScore     = r.RiskScore,
            ModelVersion  = r.ModelVersion,
            CalculatedAt  = r.CalculatedAt,
            SatelliteId   = r.SatelliteId,
            SatelliteName = r.Satellite.Name.ToString(),
            CreatedBy     = r.CreatedBy,
            CreatedAt     = r.CreatedAt
        };
        return View(vm);
    }

    // GET: Admin/RiskAssessment/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var r = await _context.RiskAssessments
            .Include(r => r.Satellite)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (r == null) return NotFound();

        var vm = new RiskAssessmentDeleteViewModel
        {
            Id            = r.Id,
            RiskScore     = r.RiskScore,
            ModelVersion  = r.ModelVersion,
            SatelliteName = r.Satellite.Name.ToString()
        };
        return View(vm);
    }

    // POST: Admin/RiskAssessment/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ra = await _context.RiskAssessments.FindAsync(id);
        if (ra != null) _context.RiskAssessments.Remove(ra);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
