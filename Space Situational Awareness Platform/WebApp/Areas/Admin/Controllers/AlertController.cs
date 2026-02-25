using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            var appDbContext = _context.Alerts.Include(a => a.ConjunctionEvent).Include(a => a.User);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/Alert/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts
                .Include(a => a.ConjunctionEvent)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // GET: Admin/Alert/Create
        public IActionResult Create()
        {
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FirstName");
            return View();
        }

        // POST: Admin/Alert/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Severity,IsAcknowledged,UserId,ConjunctionEventId,Id")] Alert alert)
        {
            if (ModelState.IsValid)
            {
                alert.Id = Guid.NewGuid();
                _context.Add(alert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status", alert.ConjunctionEventId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FirstName", alert.UserId);
            return View(alert);
        }

        // GET: Admin/Alert/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts.FindAsync(id);
            if (alert == null)
            {
                return NotFound();
            }
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status", alert.ConjunctionEventId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FirstName", alert.UserId);
            return View(alert);
        }

        // POST: Admin/Alert/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Severity,IsAcknowledged,UserId,ConjunctionEventId,Id")] Alert alert)
        {
            if (id != alert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlertExists(alert.Id))
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
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status", alert.ConjunctionEventId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FirstName", alert.UserId);
            return View(alert);
        }

        // GET: Admin/Alert/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alert = await _context.Alerts
                .Include(a => a.ConjunctionEvent)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (alert == null)
            {
                return NotFound();
            }

            return View(alert);
        }

        // POST: Admin/Alert/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var alert = await _context.Alerts.FindAsync(id);
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlertExists(Guid id)
        {
            return _context.Alerts.Any(e => e.Id == id);
        }
    }
}
