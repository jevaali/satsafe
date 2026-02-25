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
            return View(await _context.Operators.ToListAsync());
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
            return View();
        }

        // POST: Admin/Operator/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Country,Type,Id")] Operator @operator)
        {
            if (ModelState.IsValid)
            {
                @operator.Id = Guid.NewGuid();
                _context.Add(@operator);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            return View(@operator);
        }

        // POST: Admin/Operator/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Country,Type,Id")] Operator @operator)
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
    }
}
