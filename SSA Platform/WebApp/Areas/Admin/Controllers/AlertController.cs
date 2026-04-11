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
public class AlertController : Controller
{
    private readonly AppDbContext _context;

    public AlertController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/Alert
    public async Task<IActionResult> Index()
    {
        var alerts = await _context.Alerts
            .Include(a => a.ConjunctionEvent)
            .Include(a => a.User)
            .OrderBy(a => a.IsAcknowledged)
            .ThenByDescending(a => a.Severity)
            .ToListAsync();

        var vm = new AlertIndexViewModel
        {
            TotalCount          = alerts.Count,
            UnacknowledgedCount = alerts.Count(a => !a.IsAcknowledged),
            CriticalCount       = alerts.Count(a => a.Severity == EAlertSeverity.Critical),
            Alerts              = alerts.Select(a => new AlertListItemViewModel
            {
                Id                 = a.Id,
                Severity           = a.Severity,
                Message            = a.Message?.ToString() ?? string.Empty,
                UserName           = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "—",
                ConjunctionStatus  = a.ConjunctionEvent?.Status.ToString(),
                ConjunctionEventId = a.ConjunctionEventId,
                CreatedAt          = a.CreatedAt,
                IsAcknowledged     = a.IsAcknowledged
            })
        };
        return View(vm);
    }

    // GET: Admin/Alert/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var a = await _context.Alerts
            .Include(a => a.ConjunctionEvent)
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (a == null) return NotFound();

        var vm = new AlertDetailViewModel
        {
            Id                 = a.Id,
            Severity           = a.Severity,
            Message            = a.Message?.ToString() ?? string.Empty,
            UserName           = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "—",
            UserEmail          = a.User?.Email ?? "—",
            ConjunctionStatus  = a.ConjunctionEvent?.Status.ToString(),
            ConjunctionEventId = a.ConjunctionEventId,
            CreatedAt          = a.CreatedAt,
            IsAcknowledged     = a.IsAcknowledged,
            CreatedBy          = a.CreatedBy,
            UpdatedAt          = a.UpdatedAt
        };
        return View(vm);
    }

    // GET: Admin/Alert/Create
    public async Task<IActionResult> Create()
    {
        var vm = new AlertFormViewModel
        {
            UserOptions        = await BuildUserSelectListAsync(),
            ConjunctionOptions = await BuildConjunctionSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/Alert/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AlertFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var alert = new Alert
            {
                Id                 = Guid.NewGuid(),
                Severity           = vm.Severity,
                IsAcknowledged     = vm.IsAcknowledged,
                UserId             = vm.UserId,
                ConjunctionEventId = vm.ConjunctionEventId,
                CreatedBy          = User.Identity?.Name ?? "admin",
                CreatedAt          = DateTime.UtcNow
            };
            _context.Add(alert);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.UserOptions        = await BuildUserSelectListAsync(vm.UserId);
        vm.ConjunctionOptions = await BuildConjunctionSelectListAsync(vm.ConjunctionEventId);
        return View(vm);
    }

    // GET: Admin/Alert/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var a = await _context.Alerts.FindAsync(id);
        if (a == null) return NotFound();

        var vm = new AlertFormViewModel
        {
            Id                 = a.Id,
            Severity           = a.Severity,
            IsAcknowledged     = a.IsAcknowledged,
            UserId             = a.UserId,
            ConjunctionEventId = a.ConjunctionEventId,
            UserOptions        = await BuildUserSelectListAsync(a.UserId),
            ConjunctionOptions = await BuildConjunctionSelectListAsync(a.ConjunctionEventId)
        };
        return View(vm);
    }

    // POST: Admin/Alert/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, AlertFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null) return NotFound();

            alert.Severity           = vm.Severity;
            alert.IsAcknowledged     = vm.IsAcknowledged;
            alert.UserId             = vm.UserId;
            alert.ConjunctionEventId = vm.ConjunctionEventId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.UserOptions        = await BuildUserSelectListAsync(vm.UserId);
        vm.ConjunctionOptions = await BuildConjunctionSelectListAsync(vm.ConjunctionEventId);
        return View(vm);
    }

    // GET: Admin/Alert/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var a = await _context.Alerts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (a == null) return NotFound();

        var vm = new AlertDeleteViewModel
        {
            Id             = a.Id,
            Severity       = a.Severity.ToString(),
            Message        = a.Message?.ToString() ?? string.Empty,
            UserName       = a.User != null ? $"{a.User.FirstName} {a.User.LastName}".Trim() : "—",
            IsAcknowledged = a.IsAcknowledged
        };
        return View(vm);
    }

    // POST: Admin/Alert/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert != null) _context.Alerts.Remove(alert);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildUserSelectListAsync(Guid? selected = null)
    {
        var users = await _context.Users.OrderBy(u => u.Email).ToListAsync();
        var items = users.Select(u => new SelectListItem
        {
            Value    = u.Id.ToString(),
            Text     = $"{u.FirstName} {u.LastName}".Trim() + $" ({u.Email})",
            Selected = u.Id == selected
        }).ToList();
        items.Insert(0, new SelectListItem { Value = "", Text = "— Select user —" });
        return items;
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
                Text     = $"{pName} ↔ {sName} ({c.TimeOfClosestApproach:yyyy-MM-dd}) [{c.Status}]",
                Selected = c.Id == selected
            };
        }).ToList();
        items.Insert(0, new SelectListItem { Value = "", Text = "— Select conjunction event —" });
        return items;
    }
}
