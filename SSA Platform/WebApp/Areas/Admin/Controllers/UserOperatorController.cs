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
public class UserOperatorController : Controller
{
    private readonly AppDbContext _context;

    public UserOperatorController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin/UserOperator
    public async Task<IActionResult> Index()
    {
        var links = await _context.UserOperators
            .Include(u => u.AppUser)
            .Include(u => u.Operator)
            .ToListAsync();

        var vm = new UserOperatorIndexViewModel
        {
            TotalCount = links.Count,
            Links      = links.Select(u => new UserOperatorListItemViewModel
            {
                Id           = u.Id,
                UserName     = u.AppUser != null ? $"{u.AppUser.FirstName} {u.AppUser.LastName}".Trim() : "—",
                UserEmail    = u.AppUser?.Email ?? "—",
                OperatorName = u.Operator?.Name.ToString() ?? "—",
                OperatorId   = u.OperatorId
            })
        };
        return View(vm);
    }

    // GET: Admin/UserOperator/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var u = await _context.UserOperators
            .Include(u => u.AppUser)
            .Include(u => u.Operator)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (u == null) return NotFound();

        var vm = new UserOperatorDetailViewModel
        {
            Id           = u.Id,
            UserName     = u.AppUser != null ? $"{u.AppUser.FirstName} {u.AppUser.LastName}".Trim() : "—",
            UserEmail    = u.AppUser?.Email ?? "—",
            OperatorName = u.Operator?.Name.ToString() ?? "—",
            OperatorId   = u.OperatorId,
            AppUserId    = u.AppUserId
        };
        return View(vm);
    }

    // GET: Admin/UserOperator/Create
    public async Task<IActionResult> Create()
    {
        var vm = new UserOperatorFormViewModel
        {
            UserOptions     = await BuildUserSelectListAsync(),
            OperatorOptions = await BuildOperatorSelectListAsync()
        };
        return View(vm);
    }

    // POST: Admin/UserOperator/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserOperatorFormViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var link = new UserOperator
            {
                Id         = Guid.NewGuid(),
                AppUserId  = vm.AppUserId,
                OperatorId = vm.OperatorId
            };
            _context.Add(link);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.UserOptions     = await BuildUserSelectListAsync(vm.AppUserId);
        vm.OperatorOptions = await BuildOperatorSelectListAsync(vm.OperatorId);
        return View(vm);
    }

    // GET: Admin/UserOperator/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var u = await _context.UserOperators.FindAsync(id);
        if (u == null) return NotFound();

        var vm = new UserOperatorFormViewModel
        {
            Id              = u.Id,
            AppUserId       = u.AppUserId,
            OperatorId      = u.OperatorId,
            UserOptions     = await BuildUserSelectListAsync(u.AppUserId),
            OperatorOptions = await BuildOperatorSelectListAsync(u.OperatorId)
        };
        return View(vm);
    }

    // POST: Admin/UserOperator/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UserOperatorFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var u = await _context.UserOperators.FindAsync(id);
            if (u == null) return NotFound();

            u.AppUserId  = vm.AppUserId;
            u.OperatorId = vm.OperatorId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        vm.UserOptions     = await BuildUserSelectListAsync(vm.AppUserId);
        vm.OperatorOptions = await BuildOperatorSelectListAsync(vm.OperatorId);
        return View(vm);
    }

    // GET: Admin/UserOperator/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var u = await _context.UserOperators
            .Include(u => u.AppUser)
            .Include(u => u.Operator)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (u == null) return NotFound();

        var vm = new UserOperatorDeleteViewModel
        {
            Id           = u.Id,
            UserName     = u.AppUser != null ? $"{u.AppUser.FirstName} {u.AppUser.LastName}".Trim() : "—",
            OperatorName = u.Operator?.Name.ToString() ?? "—"
        };
        return View(vm);
    }

    // POST: Admin/UserOperator/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var u = await _context.UserOperators.FindAsync(id);
        if (u != null) _context.UserOperators.Remove(u);
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

    private async Task<List<SelectListItem>> BuildOperatorSelectListAsync(Guid? selected = null)
    {
        var ops = await _context.Operators.ToListAsync();
        var items = ops.OrderBy(o => o.Name.ToString()).Select(o => new SelectListItem
        {
            Value    = o.Id.ToString(),
            Text     = o.Name.ToString(),
            Selected = o.Id == selected
        }).ToList();
        items.Insert(0, new SelectListItem { Value = "", Text = "— Select operator —" });
        return items;
    }
}
