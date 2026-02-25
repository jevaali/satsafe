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
    public class RiskAssessmentController : Controller
    {
        private readonly AppDbContext _context;

        public RiskAssessmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/RiskAssessment
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.RiskAssessments.Include(r => r.ConjunctionEvent);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/RiskAssessment/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var riskAssessment = await _context.RiskAssessments
                .Include(r => r.ConjunctionEvent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (riskAssessment == null)
            {
                return NotFound();
            }

            return View(riskAssessment);
        }

        // GET: Admin/RiskAssessment/Create
        public IActionResult Create()
        {
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status");
            return View();
        }

        // POST: Admin/RiskAssessment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RiskScore,ModelVersion,CalculatedAt,ConjunctionEventId,Id")] RiskAssessment riskAssessment)
        {
            if (ModelState.IsValid)
            {
                riskAssessment.Id = Guid.NewGuid();
                _context.Add(riskAssessment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status", riskAssessment.ConjunctionEventId);
            return View(riskAssessment);
        }

        // GET: Admin/RiskAssessment/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var riskAssessment = await _context.RiskAssessments.FindAsync(id);
            if (riskAssessment == null)
            {
                return NotFound();
            }
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status", riskAssessment.ConjunctionEventId);
            return View(riskAssessment);
        }

        // POST: Admin/RiskAssessment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("RiskScore,ModelVersion,CalculatedAt,ConjunctionEventId,Id")] RiskAssessment riskAssessment)
        {
            if (id != riskAssessment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(riskAssessment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RiskAssessmentExists(riskAssessment.Id))
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
            ViewData["ConjunctionEventId"] = new SelectList(_context.ConjunctionEvents, "Id", "Status", riskAssessment.ConjunctionEventId);
            return View(riskAssessment);
        }

        // GET: Admin/RiskAssessment/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var riskAssessment = await _context.RiskAssessments
                .Include(r => r.ConjunctionEvent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (riskAssessment == null)
            {
                return NotFound();
            }

            return View(riskAssessment);
        }

        // POST: Admin/RiskAssessment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var riskAssessment = await _context.RiskAssessments.FindAsync(id);
            if (riskAssessment != null)
            {
                _context.RiskAssessments.Remove(riskAssessment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RiskAssessmentExists(Guid id)
        {
            return _context.RiskAssessments.Any(e => e.Id == id);
        }
    }
}
