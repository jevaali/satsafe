using System;
using System.Collections.Generic;
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
            var appDbContext = _context.NotificationSettings.Include(n => n.AppUser);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/NotificationSetting/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationSetting = await _context.NotificationSettings
                .Include(n => n.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificationSetting == null)
            {
                return NotFound();
            }

            return View(notificationSetting);
        }

        // GET: Admin/NotificationSetting/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName");
            return View();
        }

        // POST: Admin/NotificationSetting/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ThresholdDistance,EmailEnabled,SmsEnabled,AppUserId,Id")] NotificationSetting notificationSetting)
        {
            if (ModelState.IsValid)
            {
                notificationSetting.Id = Guid.NewGuid();
                _context.Add(notificationSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", notificationSetting.AppUserId);
            return View(notificationSetting);
        }

        // GET: Admin/NotificationSetting/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationSetting = await _context.NotificationSettings.FindAsync(id);
            if (notificationSetting == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", notificationSetting.AppUserId);
            return View(notificationSetting);
        }

        // POST: Admin/NotificationSetting/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ThresholdDistance,EmailEnabled,SmsEnabled,AppUserId,Id")] NotificationSetting notificationSetting)
        {
            if (id != notificationSetting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notificationSetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationSettingExists(notificationSetting.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", notificationSetting.AppUserId);
            return View(notificationSetting);
        }

        // GET: Admin/NotificationSetting/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notificationSetting = await _context.NotificationSettings
                .Include(n => n.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notificationSetting == null)
            {
                return NotFound();
            }

            return View(notificationSetting);
        }

        // POST: Admin/NotificationSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var notificationSetting = await _context.NotificationSettings.FindAsync(id);
            if (notificationSetting != null)
            {
                _context.NotificationSettings.Remove(notificationSetting);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationSettingExists(Guid id)
        {
            return _context.NotificationSettings.Any(e => e.Id == id);
        }
    }
}
