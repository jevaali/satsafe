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
            var appDbContext = _context.UserOperators.Include(u => u.AppUser).Include(u => u.Operator);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/UserOperator/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userOperator = await _context.UserOperators
                .Include(u => u.AppUser)
                .Include(u => u.Operator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userOperator == null)
            {
                return NotFound();
            }

            return View(userOperator);
        }

        // GET: Admin/UserOperator/Create
        public IActionResult Create()
        {
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName");
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country");
            return View();
        }

        // POST: Admin/UserOperator/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppUserId,OperatorId,Id")] UserOperator userOperator)
        {
            if (ModelState.IsValid)
            {
                userOperator.Id = Guid.NewGuid();
                _context.Add(userOperator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", userOperator.AppUserId);
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country", userOperator.OperatorId);
            return View(userOperator);
        }

        // GET: Admin/UserOperator/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userOperator = await _context.UserOperators.FindAsync(id);
            if (userOperator == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", userOperator.AppUserId);
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country", userOperator.OperatorId);
            return View(userOperator);
        }

        // POST: Admin/UserOperator/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AppUserId,OperatorId,Id")] UserOperator userOperator)
        {
            if (id != userOperator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userOperator);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserOperatorExists(userOperator.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", userOperator.AppUserId);
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country", userOperator.OperatorId);
            return View(userOperator);
        }

        // GET: Admin/UserOperator/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userOperator = await _context.UserOperators
                .Include(u => u.AppUser)
                .Include(u => u.Operator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userOperator == null)
            {
                return NotFound();
            }

            return View(userOperator);
        }

        // POST: Admin/UserOperator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userOperator = await _context.UserOperators.FindAsync(id);
            if (userOperator != null)
            {
                _context.UserOperators.Remove(userOperator);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserOperatorExists(Guid id)
        {
            return _context.UserOperators.Any(e => e.Id == id);
        }
    }
}
