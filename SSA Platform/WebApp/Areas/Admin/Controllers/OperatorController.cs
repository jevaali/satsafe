using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Domain;
using App.Domain.Entities;
using App.Infrastructure.Persistence;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var operators = _context.Operators
                .Include(o => o.Satellites)
                .Include(o => o.UserOperators);
            return View(await operators.ToListAsync());
        }

        // GET: Admin/Operator/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @operator = await _context.Operators
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@operator == null)
            {
                return NotFound();
            }

            return View(@operator);
        }

        // GET: Admin/Operator/Create
        public IActionResult Create()
        {
            ViewBag.Countries = GetCountrySelectList();
            return View();
        }

        // POST: Admin/Operator/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,CountryCode,OperatorType,Id")] App.Domain.Entities.Operator @operator)
        {
            if (ModelState.IsValid)
            {
                @operator.Id = Guid.NewGuid();
                @operator.CreatedBy = User.Identity?.Name ?? "admin";
                @operator.CreatedAt = DateTime.UtcNow;
                _context.Add(@operator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Countries = GetCountrySelectList(@operator.CountryCode);
            return View(@operator);
        }

        // GET: Admin/Operator/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @operator = await _context.Operators.FindAsync(id);
            if (@operator == null)
            {
                return NotFound();
            }
            ViewBag.Countries = GetCountrySelectList(@operator.CountryCode);
            return View(@operator);
        }

        // POST: Admin/Operator/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,CountryCode,OperatorType,Id")] App.Domain.Entities.Operator @operator)
        {
            if (id != @operator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@operator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperatorExists(@operator.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Countries = GetCountrySelectList(@operator.CountryCode);
            return View(@operator);
        }

        // GET: Admin/Operator/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @operator = await _context.Operators
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@operator == null)
            {
                return NotFound();
            }

            return View(@operator);
        }

        // POST: Admin/Operator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var @operator = await _context.Operators.FindAsync(id);
            if (@operator != null)
            {
                _context.Operators.Remove(@operator);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OperatorExists(Guid id)
        {
            return _context.Operators.Any(e => e.Id == id);
        }

        private static List<SelectListItem> GetCountrySelectList(string? selectedCode = null)
        {
            var seen = new HashSet<string>();
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
                    Value = r!.TwoLetterISORegionName,
                    Text  = r.DisplayName,
                    Selected = string.Equals(r.TwoLetterISORegionName, selectedCode,
                                             StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "— Select country —" });
            return items;
        }
    }
}
