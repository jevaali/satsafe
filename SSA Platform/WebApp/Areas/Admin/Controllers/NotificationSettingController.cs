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
public class NotificationSettingController : Controller
{
    private readonly AppDbContext _context;

    public NotificationSettingController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/NotificationSetting
    public async Task<IActionResult> Index()
    {
        var settings = await _context.NotificationSettings
            .Include(n => n.AppUser)
            .ToListAsync();

        var vm = new NotificationSettingIndexViewModel
        {
            TotalCount = settings.Count,
            Settings   = settings.Select(n => new NotificationSettingListItemViewModel
            {
                Id                    = n.Id,
                UserName              = n.AppUser != null ? $"{n.AppUser.FirstName} {n.AppUser.LastName}".Trim() : "—",
                ConjunctionThresholdKm= n.ConjunctionThresholdKm,
                AlertThresholdKm      = n.AlertThresholdKm,
                EmailEnabled          = n.EmailEnabled,
                SmsEnabled            = n.SmsEnabled
            })
        };
        return View(vm);
    }

    // GET: Admin/NotificationSetting/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var n = await _context.NotificationSettings
            .Include(n => n.AppUser)
            .FirstOrDefaultAsync(n => n.Id == id);
        if (n == null) return NotFound();

        var vm = new NotificationSettingDetailViewModel
        {
            Id                    = n.Id,
            UserName              = n.AppUser != null ? $"{n.AppUser.FirstName} {n.AppUser.LastName}".Trim() : "—",
            UserEmail             = n.AppUser?.Email ?? "—",
            ConjunctionThresholdKm= n.ConjunctionThresholdKm,
            AlertThresholdKm      = n.AlertThresholdKm,
            EmailEnabled          = n.EmailEnabled,
            SmsEnabled            = n.SmsEnabled,
            CreatedBy             = n.CreatedBy,
            CreatedAt             = n.CreatedAt,
            UpdatedAt             = n.UpdatedAt
        };
        return View(vm);
    }

    // GET: Admin/NotificationSetting/Create
    public async Task<IActionResult> Create()
    {
        var vm = new NotificationSettingFormViewModel
        {
            UserOptions = await BuildUserSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/NotificationSetting/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotificationSettingFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var setting = new NotificationSetting
            {
                Id                    = Guid.NewGuid(),
                AppUserId             = vm.AppUserId,
                ConjunctionThresholdKm= vm.ConjunctionThresholdKm,
                AlertThresholdKm      = vm.AlertThresholdKm,
                EmailEnabled          = vm.EmailEnabled,
                SmsEnabled            = vm.SmsEnabled,
                CreatedBy             = User.Identity?.Name ?? "admin",
                CreatedAt             = DateTime.UtcNow
            };
            _context.Add(setting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.UserOptions = await BuildUserSelectListAsync(vm.AppUserId);
        return View(vm);
    }

    // GET: Admin/NotificationSetting/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var n = await _context.NotificationSettings.FindAsync(id);
        if (n == null) return NotFound();

        var vm = new NotificationSettingFormViewModel
        {
            Id                    = n.Id,
            AppUserId             = n.AppUserId,
            ConjunctionThresholdKm= n.ConjunctionThresholdKm,
            AlertThresholdKm      = n.AlertThresholdKm,
            EmailEnabled          = n.EmailEnabled,
            SmsEnabled            = n.SmsEnabled,
            UserOptions           = await BuildUserSelectListAsync(n.AppUserId)
        };
        return View(vm);
    }

    // POST: Admin/NotificationSetting/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, NotificationSettingFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var n = await _context.NotificationSettings.FindAsync(id);
            if (n == null) return NotFound();

            n.AppUserId              = vm.AppUserId;
            n.ConjunctionThresholdKm = vm.ConjunctionThresholdKm;
            n.AlertThresholdKm       = vm.AlertThresholdKm;
            n.EmailEnabled           = vm.EmailEnabled;
            n.SmsEnabled             = vm.SmsEnabled;
            n.UpdatedBy              = User.Identity?.Name ?? "admin";
            n.UpdatedAt              = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.UserOptions = await BuildUserSelectListAsync(vm.AppUserId);
        return View(vm);
    }

    // GET: Admin/NotificationSetting/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var n = await _context.NotificationSettings
            .Include(n => n.AppUser)
            .FirstOrDefaultAsync(n => n.Id == id);
        if (n == null) return NotFound();

        var vm = new NotificationSettingDeleteViewModel
        {
            Id                    = n.Id,
            UserName              = n.AppUser != null ? $"{n.AppUser.FirstName} {n.AppUser.LastName}".Trim() : "—",
            ConjunctionThresholdKm= n.ConjunctionThresholdKm
        };
        return View(vm);
    }

    // POST: Admin/NotificationSetting/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var n = await _context.NotificationSettings.FindAsync(id);
        if (n != null) _context.NotificationSettings.Remove(n);
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
}
