using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            .Include(r => r.ConjunctionEvent)
            .OrderByDescending(r => r.RiskScore)
            .ToListAsync();

        var vm = new RiskAssessmentIndexViewModel
        {
            TotalCount    = assessments.Count,
            HighRiskCount = assessments.Count(r => r.RiskScore > 0.7),
            Assessments   = assessments.Select(r => new RiskAssessmentListItemViewModel
            {
                Id                 = r.Id,
                RiskScore          = r.RiskScore,
                ModelVersion       = r.ModelVersion,
                ConjunctionStatus  = r.ConjunctionEvent?.Status.ToString(),
                ConjunctionEventId = r.ConjunctionEventId
            })
        };
        return View(vm);
    }

    // GET: Admin/RiskAssessment/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var r = await _context.RiskAssessments
            .Include(r => r.ConjunctionEvent)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (r == null) return NotFound();

        var vm = new RiskAssessmentDetailViewModel
        {
            Id                 = r.Id,
            RiskScore          = r.RiskScore,
            ModelVersion       = r.ModelVersion,
            ConjunctionStatus  = r.ConjunctionEvent?.Status.ToString(),
            ConjunctionEventId = r.ConjunctionEventId,
            CreatedBy          = r.CreatedBy,
            CreatedAt          = r.CreatedAt
        };
        return View(vm);
    }

    // GET: Admin/RiskAssessment/Create
    public async Task<IActionResult> Create()
    {
        var vm = new RiskAssessmentFormViewModel
        {
            ConjunctionOptions = await BuildConjunctionSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/RiskAssessment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RiskAssessmentFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var conjEvent = await _context.ConjunctionEvents.FindAsync(vm.ConjunctionEventId);
            if (conjEvent == null)
            {
                ModelState.AddModelError(nameof(vm.ConjunctionEventId), "Conjunction event not found.");
                vm.ConjunctionOptions = await BuildConjunctionSelectListAsync(vm.ConjunctionEventId);
                return View(vm);
            }

            var ra = new RiskAssessment
            {
                Id                 = Guid.NewGuid(),
                RiskScore          = vm.RiskScore,
                ModelVersion       = vm.ModelVersion,
                ConjunctionEventId = vm.ConjunctionEventId,
                ConjunctionEvent   = conjEvent,
                CreatedBy          = User.Identity?.Name ?? "admin",
                CreatedAt          = DateTime.UtcNow
            };
            _context.Add(ra);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.ConjunctionOptions = await BuildConjunctionSelectListAsync(vm.ConjunctionEventId);
        return View(vm);
    }

    // GET: Admin/RiskAssessment/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var r = await _context.RiskAssessments.FindAsync(id);
        if (r == null) return NotFound();

        var vm = new RiskAssessmentFormViewModel
        {
            Id                 = r.Id,
            RiskScore          = r.RiskScore,
            ModelVersion       = r.ModelVersion,
            ConjunctionEventId = r.ConjunctionEventId,
            ConjunctionOptions = await BuildConjunctionSelectListAsync(r.ConjunctionEventId)
        };
        return View(vm);
    }

    // POST: Admin/RiskAssessment/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RiskAssessmentFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var ra = await _context.RiskAssessments.FindAsync(id);
            if (ra == null) return NotFound();

            ra.UpdateRiskScore(vm.RiskScore);
            ra.ModelVersion       = vm.ModelVersion;
            ra.ConjunctionEventId = vm.ConjunctionEventId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.ConjunctionOptions = await BuildConjunctionSelectListAsync(vm.ConjunctionEventId);
        return View(vm);
    }

    // GET: Admin/RiskAssessment/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var r = await _context.RiskAssessments
            .Include(r => r.ConjunctionEvent)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (r == null) return NotFound();

        var vm = new RiskAssessmentDeleteViewModel
        {
            Id                = r.Id,
            RiskScore         = r.RiskScore,
            ModelVersion      = r.ModelVersion,
            ConjunctionStatus = r.ConjunctionEvent?.Status.ToString()
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

    private async Task<List<SelectListItem>> BuildConjunctionSelectListAsync(Guid? selected = null)
    {
        var events = await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .OrderByDescending(c => c.TimeOfClosestApproach)
            .ToListAsync();

        var items = events.Select(c =>
        {
            var pName = c.PrimaryObject is Satellite ps ? ps.Name.ToString() : $"DEBRIS-{c.PrimaryObject?.NoradId}";
            var sName = c.SecondaryObject is Satellite ss ? ss.Name.ToString() : $"DEBRIS-{c.SecondaryObject?.NoradId}";
            return new SelectListItem
            {
                Value    = c.Id.ToString(),
                Text     = $"{pName} ↔ {sName} [{c.Status}]",
                Selected = c.Id == selected
            };
        }).ToList();
        items.Insert(0, new SelectListItem { Value = "", Text = "— Select conjunction event —" });
        return items;
    }
}
