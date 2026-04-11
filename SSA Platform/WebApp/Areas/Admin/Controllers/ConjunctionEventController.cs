using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ConjunctionEventController : Controller
{
    private readonly AppDbContext _context;

    public ConjunctionEventController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/ConjunctionEvent
    public async Task<IActionResult> Index()
    {
        var events = await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .OrderByDescending(c => c.CollisionProbability > 0.01 || c.MissDistance < 1000)
            .ThenBy(c => c.TimeOfClosestApproach)
            .ToListAsync();

        var vm = new ConjunctionEventIndexViewModel
        {
            TotalCount    = events.Count,
            HighRiskCount = events.Count(c => c.IsHighRisk()),
            PendingCount  = events.Count(c => c.Status == EConjunctionEventStatus.Pending),
            Events        = events.Select(c => new ConjunctionEventListItemViewModel
            {
                Id                  = c.Id,
                PrimaryObjectName   = ResolveName(c.PrimaryObject),
                SecondaryObjectName = ResolveName(c.SecondaryObject),
                Tca                 = c.TimeOfClosestApproach,
                MissDistance        = c.MissDistance,
                CollisionProbability= c.CollisionProbability,
                Status              = c.Status,
                IsHighRisk          = c.IsHighRisk()
            })
        };
        return View(vm);
    }

    // GET: Admin/ConjunctionEvent/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var c = await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .Include(c => c.RiskAssessment)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (c == null) return NotFound();

        var vm = new ConjunctionEventDetailViewModel
        {
            Id                         = c.Id,
            PrimaryObjectName          = ResolveName(c.PrimaryObject),
            SecondaryObjectName        = ResolveName(c.SecondaryObject),
            PrimaryObjectId            = c.PrimaryObjectId,
            SecondaryObjectId          = c.SecondaryObjectId,
            Tca                        = c.TimeOfClosestApproach,
            MissDistance               = c.MissDistance,
            CollisionProbability       = c.CollisionProbability,
            Status                     = c.Status,
            IsHighRisk                 = c.IsHighRisk(),
            PrimaryAcknowledgement     = c.PrimaryAcknowledgement.ToString(),
            SecondaryAcknowledgement   = c.SecondaryAcknowledgement.ToString(),
            RiskModelVersion           = c.RiskAssessment?.ModelVersion,
            RiskScore                  = c.RiskAssessment?.RiskScore,
            CreatedBy                  = c.CreatedBy,
            CreatedAt                  = c.CreatedAt
        };
        return View(vm);
    }

    // GET: Admin/ConjunctionEvent/Create
    public async Task<IActionResult> Create()
    {
        var vm = new ConjunctionEventFormViewModel
        {
            SpaceObjectOptions = await BuildSpaceObjectSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/ConjunctionEvent/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConjunctionEventFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var ce = new ConjunctionEvent(
                vm.PrimaryObjectId,
                vm.SecondaryObjectId,
                vm.TimeOfClosestApproach,
                vm.MissDistance,
                threshold: 5000);

            ce.UpdateCollisionProbability(vm.CollisionProbability);
            ce.Status = vm.Status;

            _context.Add(ce);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.SpaceObjectOptions = await BuildSpaceObjectSelectListAsync();
        return View(vm);
    }

    // GET: Admin/ConjunctionEvent/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var c = await _context.ConjunctionEvents.FindAsync(id);
        if (c == null) return NotFound();

        var vm = new ConjunctionEventFormViewModel
        {
            Id                    = c.Id,
            PrimaryObjectId       = c.PrimaryObjectId,
            SecondaryObjectId     = c.SecondaryObjectId,
            TimeOfClosestApproach = c.TimeOfClosestApproach,
            MissDistance          = c.MissDistance,
            CollisionProbability  = c.CollisionProbability,
            Status                = c.Status,
            SpaceObjectOptions    = await BuildSpaceObjectSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/ConjunctionEvent/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ConjunctionEventFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var ce = await _context.ConjunctionEvents.FindAsync(id);
            if (ce == null) return NotFound();

            ce.PrimaryObjectId       = vm.PrimaryObjectId;
            ce.SecondaryObjectId     = vm.SecondaryObjectId;
            ce.TimeOfClosestApproach = vm.TimeOfClosestApproach;
            ce.UpdateMissDistance(vm.MissDistance);
            ce.UpdateCollisionProbability(vm.CollisionProbability);
            ce.Status = vm.Status;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.SpaceObjectOptions = await BuildSpaceObjectSelectListAsync();
        return View(vm);
    }

    // GET: Admin/ConjunctionEvent/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var c = await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (c == null) return NotFound();

        var vm = new ConjunctionEventDeleteViewModel
        {
            Id                  = c.Id,
            PrimaryObjectName   = ResolveName(c.PrimaryObject),
            SecondaryObjectName = ResolveName(c.SecondaryObject),
            Tca                 = c.TimeOfClosestApproach,
            Status              = c.Status.ToString()
        };
        return View(vm);
    }

    // POST: Admin/ConjunctionEvent/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ce = await _context.ConjunctionEvents.FindAsync(id);
        if (ce != null) _context.ConjunctionEvents.Remove(ce);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static string ResolveName(SpaceObject? obj) =>
        obj switch
        {
            Satellite s    => s.Name.ToString(),
            DebrisObject d => $"DEBRIS-{d.NoradId}",
            null           => "—",
            _              => obj.NoradId.ToString()
        };

    private async Task<List<SelectListItem>> BuildSpaceObjectSelectListAsync()
    {
        var objects = await _context.SpaceObjects
            .OfType<Satellite>()
            .ToListAsync();
        objects = objects.OrderBy(s => s.Name.ToString()).ToList();

        var debris = await _context.SpaceObjects
            .OfType<DebrisObject>()
            .OrderBy(d => d.NoradId)
            .ToListAsync();

        var items = new List<SelectListItem>();
        items.Add(new SelectListItem { Value = "", Text = "— Select object —" });

        if (objects.Any())
        {
            var satGroup = new SelectListGroup { Name = "Satellites" };
            foreach (var s in objects)
                items.Add(new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Name} (NORAD {s.NoradId})", Group = satGroup });
        }

        if (debris.Any())
        {
            var debGroup = new SelectListGroup { Name = "Debris" };
            foreach (var d in debris)
                items.Add(new SelectListItem { Value = d.Id.ToString(), Text = $"DEBRIS-{d.NoradId}", Group = debGroup });
        }

        return items;
    }
}
