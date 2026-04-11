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
public class SatelliteController : Controller
{
    private readonly AppDbContext _context;

    public SatelliteController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Satellite
    public async Task<IActionResult> Index()
    {
        var satellites = await _context.Satellites
            .Include(s => s.Orbit)
            .Include(s => s.Operator)
            .ToListAsync();
        satellites = satellites.OrderBy(s => s.Name.ToString()).ToList();

        var vm = new SatelliteIndexViewModel
        {
            TotalCount   = satellites.Count,
            ActiveCount  = satellites.Count(s => s.Status == ESatelliteStatus.Active),
            DeletedCount = satellites.Count(s => s.Status == ESatelliteStatus.Deleted),
            Satellites   = satellites.Select(s => new SatelliteListItemViewModel
            {
                Id               = s.Id,
                Name             = s.Name.ToString(),
                NoradId          = s.NoradId,
                LaunchDate       = s.LaunchDate,
                Status           = s.Status,
                OperatorName     = s.Operator?.Name.ToString() ?? s.OperatorName ?? "—",
                SemiMajorAxis    = s.Orbit?.Parameters.SemiMajorAxis,
                Inclination      = s.Orbit?.Parameters.Inclination,
                RAAN             = s.Orbit?.Parameters.RAAN,
                ArgumentOfPerigee= s.Orbit?.Parameters.ArgumentOfPerigee,
                MeanAnomaly      = s.Orbit?.Parameters.MeanAnomaly
            })
        };
        return View(vm);
    }

    // GET: Admin/Satellite/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var s = await _context.Satellites
            .Include(s => s.Orbit)
            .Include(s => s.Operator)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (s == null) return NotFound();

        var vm = new SatelliteDetailViewModel
        {
            Id               = s.Id,
            Name             = s.Name.ToString(),
            NoradId          = s.NoradId,
            LaunchDate       = s.LaunchDate,
            Status           = s.Status,
            OperatorName     = s.Operator?.Name.ToString() ?? s.OperatorName ?? "—",
            OperatorId       = s.OperatorId,
            SemiMajorAxis    = s.Orbit?.Parameters.SemiMajorAxis,
            Eccentricity     = s.Orbit?.Parameters.Eccentricity,
            Inclination      = s.Orbit?.Parameters.Inclination,
            RAAN             = s.Orbit?.Parameters.RAAN,
            ArgumentOfPerigee= s.Orbit?.Parameters.ArgumentOfPerigee,
            MeanAnomaly      = s.Orbit?.Parameters.MeanAnomaly,
            CreatedBy        = s.CreatedBy,
            CreatedAt        = s.CreatedAt,
            UpdatedBy        = s.UpdatedBy,
            UpdatedAt        = s.UpdatedAt
        };
        return View(vm);
    }

    // GET: Admin/Satellite/Create
    public async Task<IActionResult> Create()
    {
        var vm = new SatelliteFormViewModel
        {
            LaunchDate       = DateTime.UtcNow,
            OperatorOptions  = await BuildOperatorSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/Satellite/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SatelliteFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            Satellite sat;
            if (vm.OperatorId.HasValue)
            {
                var op = await _context.Operators.FindAsync(vm.OperatorId.Value);
                if (op != null)
                {
                    sat = Satellite.Create(vm.Name, vm.LaunchDate, op);
                }
                else
                {
                    sat = Satellite.CreateFromExternalData(vm.Name, vm.LaunchDate, null, null);
                }
            }
            else
            {
                sat = Satellite.CreateFromExternalData(vm.Name, vm.LaunchDate, null, null);
            }

            sat.NoradId = vm.NoradId;

            // Apply requested status if different from default Active
            if (vm.Status != ESatelliteStatus.Active)
            {
                _context.Add(sat);
                await _context.SaveChangesAsync();
                // Use EF entry to set private Status
                _context.Entry(sat).CurrentValues.SetValues(new { Status = vm.Status });
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.Add(sat);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        vm.OperatorOptions = await BuildOperatorSelectListAsync(vm.OperatorId);
        return View(vm);
    }

    // GET: Admin/Satellite/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var s = await _context.Satellites
            .Include(s => s.Operator)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (s == null) return NotFound();

        var vm = new SatelliteFormViewModel
        {
            Id              = s.Id,
            Name            = s.Name.ToString(),
            NoradId         = s.NoradId,
            LaunchDate      = s.LaunchDate,
            Status          = s.Status,
            OperatorId      = s.OperatorId,
            OperatorOptions = await BuildOperatorSelectListAsync(s.OperatorId)
        };
        return View(vm);
    }

    // POST: Admin/Satellite/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SatelliteFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var sat = await _context.Satellites.FindAsync(id);
            if (sat == null) return NotFound();

            // Update properties through EF change tracking (handles private setters)
            _context.Entry(sat).CurrentValues.SetValues(new
            {
                NoradId    = vm.NoradId,
                LaunchDate = vm.LaunchDate,
                Status     = vm.Status,
                OperatorId = vm.OperatorId
            });

            // Name and OperatorId need LangStr conversion
            var nameValue = (Base.Domain.LangStr)vm.Name;
            _context.Entry(sat).Property("Name").CurrentValue = nameValue;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.OperatorOptions = await BuildOperatorSelectListAsync(vm.OperatorId);
        return View(vm);
    }

    // GET: Admin/Satellite/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var s = await _context.Satellites
            .Include(s => s.Operator)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (s == null) return NotFound();

        var vm = new SatelliteDeleteViewModel
        {
            Id           = s.Id,
            Name         = s.Name.ToString(),
            NoradId      = s.NoradId,
            LaunchDate   = s.LaunchDate,
            Status       = s.Status.ToString(),
            OperatorName = s.Operator?.Name.ToString() ?? s.OperatorName ?? "—"
        };
        return View(vm);
    }

    // POST: Admin/Satellite/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var sat = await _context.Satellites.FindAsync(id);
        if (sat != null)
        {
            sat.SoftDelete(User.Identity?.Name ?? "admin");
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildOperatorSelectListAsync(Guid? selected = null)
    {
        var ops = await _context.Operators.ToListAsync();
        var items = ops.OrderBy(o => o.Name.ToString()).Select(o => new SelectListItem
        {
            Value    = o.Id.ToString(),
            Text     = o.Name.ToString(),
            Selected = o.Id == selected
        }).ToList();
        items.Insert(0, new SelectListItem { Value = "", Text = "— No operator —" });
        return items;
    }
}
