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
public class TleRecordController : Controller
{
    private readonly AppDbContext _context;

    public TleRecordController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/TleRecord
    public async Task<IActionResult> Index()
    {
        var records = await _context.TleRecords
            .Include(t => t.SpaceObject)
            .OrderByDescending(t => t.Epoch)
            .ToListAsync();

        var vm = new TleRecordIndexViewModel
        {
            TotalCount = records.Count,
            Records    = records.Select(t => new TleRecordListItemViewModel
            {
                Id               = t.Id,
                SpaceObjectNorad = t.SpaceObject?.NoradId.ToString() ?? "—",
                Epoch            = t.Epoch,
                Inclination      = t.Inclination,
                RAAN             = t.RAAN,
                Eccentricity     = t.Eccentricity,
                MeanMotion       = t.MeanMotion
            })
        };
        return View(vm);
    }

    // GET: Admin/TleRecord/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var t = await _context.TleRecords
            .Include(t => t.SpaceObject)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (t == null) return NotFound();

        var vm = new TleRecordDetailViewModel
        {
            Id               = t.Id,
            SpaceObjectNorad = t.SpaceObject?.NoradId.ToString() ?? "—",
            SpaceObjectId    = t.SpaceObjectId,
            Epoch            = t.Epoch,
            Inclination      = t.Inclination,
            RAAN             = t.RAAN,
            Eccentricity     = t.Eccentricity,
            ArgumentOfPerigee= t.ArgumentOfPerigee,
            MeanAnomaly      = t.MeanAnomaly,
            MeanMotion       = t.MeanMotion,
            RevolutionNumber = t.RevolutionNumber,
            BSTar            = t.BSTar,
            FirstDerivative  = t.FirstDerivative,
            SecondDerivative = t.SecondDerivative,
            RawLine1         = t.RawLine1,
            RawLine2         = t.RawLine2,
            CreatedBy        = t.CreatedBy,
            CreatedAt        = t.CreatedAt
        };
        return View(vm);
    }

    // GET: Admin/TleRecord/Create
    public async Task<IActionResult> Create()
    {
        var vm = new TleRecordFormViewModel
        {
            SpaceObjectOptions = await BuildSpaceObjectSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/TleRecord/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TleRecordFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var spaceObj = await _context.SpaceObjects.FindAsync(vm.SpaceObjectId);
            if (spaceObj == null)
            {
                ModelState.AddModelError(nameof(vm.SpaceObjectId), "Space object not found.");
                vm.SpaceObjectOptions = await BuildSpaceObjectSelectListAsync(vm.SpaceObjectId);
                return View(vm);
            }

            var tle = new TleRecord
            {
                Id               = Guid.NewGuid(),
                SpaceObject      = spaceObj,
                SpaceObjectId    = vm.SpaceObjectId,
                Epoch            = vm.Epoch,
                Inclination      = vm.Inclination,
                RAAN             = vm.RAAN,
                Eccentricity     = vm.Eccentricity,
                ArgumentOfPerigee= vm.ArgumentOfPerigee,
                MeanAnomaly      = vm.MeanAnomaly,
                MeanMotion       = vm.MeanMotion,
                RevolutionNumber = vm.RevolutionNumber,
                BSTar            = vm.BSTar,
                FirstDerivative  = vm.FirstDerivative,
                SecondDerivative = vm.SecondDerivative,
                RawLine1         = vm.RawLine1,
                RawLine2         = vm.RawLine2,
                CreatedBy        = User.Identity?.Name ?? "admin",
                CreatedAt        = DateTime.UtcNow
            };
            _context.Add(tle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.SpaceObjectOptions = await BuildSpaceObjectSelectListAsync(vm.SpaceObjectId);
        return View(vm);
    }

    // GET: Admin/TleRecord/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var t = await _context.TleRecords.FindAsync(id);
        if (t == null) return NotFound();

        var vm = new TleRecordFormViewModel
        {
            Id               = t.Id,
            SpaceObjectId    = t.SpaceObjectId,
            Epoch            = t.Epoch,
            Inclination      = t.Inclination,
            RAAN             = t.RAAN,
            Eccentricity     = t.Eccentricity,
            ArgumentOfPerigee= t.ArgumentOfPerigee,
            MeanAnomaly      = t.MeanAnomaly,
            MeanMotion       = t.MeanMotion,
            RevolutionNumber = t.RevolutionNumber,
            BSTar            = t.BSTar,
            FirstDerivative  = t.FirstDerivative,
            SecondDerivative = t.SecondDerivative,
            RawLine1         = t.RawLine1,
            RawLine2         = t.RawLine2,
            SpaceObjectOptions = await BuildSpaceObjectSelectListAsync(t.SpaceObjectId)
        };
        return View(vm);
    }

    // POST: Admin/TleRecord/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TleRecordFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var t = await _context.TleRecords.FindAsync(id);
            if (t == null) return NotFound();

            t.SpaceObjectId    = vm.SpaceObjectId;
            t.Epoch            = vm.Epoch;
            t.Inclination      = vm.Inclination;
            t.RAAN             = vm.RAAN;
            t.Eccentricity     = vm.Eccentricity;
            t.ArgumentOfPerigee= vm.ArgumentOfPerigee;
            t.MeanAnomaly      = vm.MeanAnomaly;
            t.MeanMotion       = vm.MeanMotion;
            t.RevolutionNumber = vm.RevolutionNumber;
            t.BSTar            = vm.BSTar;
            t.FirstDerivative  = vm.FirstDerivative;
            t.SecondDerivative = vm.SecondDerivative;
            t.RawLine1         = vm.RawLine1;
            t.RawLine2         = vm.RawLine2;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.SpaceObjectOptions = await BuildSpaceObjectSelectListAsync(vm.SpaceObjectId);
        return View(vm);
    }

    // GET: Admin/TleRecord/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var t = await _context.TleRecords
            .Include(t => t.SpaceObject)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (t == null) return NotFound();

        var vm = new TleRecordDeleteViewModel
        {
            Id               = t.Id,
            SpaceObjectNorad = t.SpaceObject?.NoradId.ToString() ?? "—",
            Epoch            = t.Epoch,
            MeanMotion       = t.MeanMotion
        };
        return View(vm);
    }

    // POST: Admin/TleRecord/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var t = await _context.TleRecords.FindAsync(id);
        if (t != null) _context.TleRecords.Remove(t);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildSpaceObjectSelectListAsync(Guid? selected = null)
    {
        var sats = await _context.SpaceObjects
            .OfType<Satellite>()
            .ToListAsync();
        sats = sats.OrderBy(s => s.Name.ToString()).ToList();

        var debris = await _context.SpaceObjects
            .OfType<DebrisObject>()
            .OrderBy(d => d.NoradId)
            .ToListAsync();

        var items = new List<SelectListItem>();
        items.Add(new SelectListItem { Value = "", Text = "— Select object —" });

        if (sats.Any())
        {
            var satGroup = new SelectListGroup { Name = "Satellites" };
            foreach (var s in sats)
                items.Add(new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Name} (NORAD {s.NoradId})", Group = satGroup, Selected = s.Id == selected });
        }

        if (debris.Any())
        {
            var debGroup = new SelectListGroup { Name = "Debris" };
            foreach (var d in debris)
                items.Add(new SelectListItem { Value = d.Id.ToString(), Text = $"DEBRIS-{d.NoradId}", Group = debGroup, Selected = d.Id == selected });
        }

        return items;
    }
}
