using System.Globalization;
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
public class OperatorController : Controller
{
    private readonly AppDbContext _context;

    public OperatorController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Operator
    public async Task<IActionResult> Index()
    {
        var operators = await _context.Operators
            .Include(o => o.Satellites)
            .Include(o => o.UserOperators)
            .ToListAsync();
        operators = operators.OrderBy(o => o.Name.ToString()).ToList();

        var vm = new OperatorIndexViewModel
        {
            TotalCount = operators.Count,
            Operators  = operators.Select(o => new OperatorListItemViewModel
            {
                Id             = o.Id,
                Name           = o.Name.ToString(),
                CountryCode    = o.CountryCode,
                OperatorType   = o.OperatorType?.ToString(),
                SatelliteCount = o.Satellites.Count,
                UserCount      = o.UserOperators.Count
            })
        };
        return View(vm);
    }

    // GET: Admin/Operator/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var op = await _context.Operators
            .Include(o => o.Satellites)
            .Include(o => o.UserOperators)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (op == null) return NotFound();

        var vm = new OperatorDetailViewModel
        {
            Id             = op.Id,
            Name           = op.Name.ToString(),
            CountryCode    = op.CountryCode,
            OperatorType   = op.OperatorType?.ToString(),
            SatelliteCount = op.Satellites.Count,
            UserCount      = op.UserOperators.Count,
            CreatedBy      = op.CreatedBy,
            CreatedAt      = op.CreatedAt,
            UpdatedBy      = op.UpdatedBy,
            UpdatedAt      = op.UpdatedAt
        };
        return View(vm);
    }

    // GET: Admin/Operator/Create
    public IActionResult Create()
    {
        var vm = new OperatorFormViewModel
        {
            CountryOptions = BuildCountrySelectList()
        };
        return View(vm);
    }

    // POST: Admin/Operator/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OperatorFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var op = new App.Domain.Entities.Operator
            {
                Id           = Guid.NewGuid(),
                Name         = vm.Name,
                CountryCode  = vm.CountryCode,
                OperatorType = vm.OperatorType,
                CreatedBy    = User.Identity?.Name ?? "admin",
                CreatedAt    = DateTime.UtcNow
            };
            _context.Add(op);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.CountryOptions = BuildCountrySelectList(vm.CountryCode);
        return View(vm);
    }

    // GET: Admin/Operator/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var op = await _context.Operators.FindAsync(id);
        if (op == null) return NotFound();

        var vm = new OperatorFormViewModel
        {
            Id             = op.Id,
            Name           = op.Name.ToString(),
            CountryCode    = op.CountryCode,
            OperatorType   = op.OperatorType,
            CountryOptions = BuildCountrySelectList(op.CountryCode)
        };
        return View(vm);
    }

    // POST: Admin/Operator/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OperatorFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var op = await _context.Operators.FindAsync(id);
            if (op == null) return NotFound();

            op.Name         = vm.Name;
            op.CountryCode  = vm.CountryCode;
            op.OperatorType = vm.OperatorType;
            op.UpdatedBy    = User.Identity?.Name ?? "admin";
            op.UpdatedAt    = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.CountryOptions = BuildCountrySelectList(vm.CountryCode);
        return View(vm);
    }

    // GET: Admin/Operator/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var op = await _context.Operators
            .Include(o => o.Satellites)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (op == null) return NotFound();

        var vm = new OperatorDeleteViewModel
        {
            Id             = op.Id,
            Name           = op.Name.ToString(),
            CountryCode    = op.CountryCode,
            OperatorType   = op.OperatorType?.ToString(),
            SatelliteCount = op.Satellites.Count
        };
        return View(vm);
    }

    // POST: Admin/Operator/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var op = await _context.Operators.FindAsync(id);
        if (op != null) _context.Operators.Remove(op);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static List<SelectListItem> BuildCountrySelectList(string? selectedCode = null)
    {
        var seen  = new HashSet<string>();
        var items = CultureInfo
            .GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch { return null; }
            })
            .Where(r => r != null && seen.Add(r.TwoLetterISORegionName))
            .OrderBy(r => r!.DisplayName)
            .Select(r => new SelectListItem
            {
                Value    = r!.TwoLetterISORegionName,
                Text     = r.DisplayName,
                Selected = string.Equals(r.TwoLetterISORegionName, selectedCode, StringComparison.OrdinalIgnoreCase)
            })
            .ToList();

        items.Insert(0, new SelectListItem { Value = "", Text = "— Select country —" });
        return items;
    }
}
