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
    public class ConjunctionEventController : Controller
    {
        private readonly AppDbContext _context;

        public ConjunctionEventController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ConjunctionEvent
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.ConjunctionEvents.Include(c => c.PrimaryObject).Include(c => c.SecondaryObject);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/ConjunctionEvent/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conjunctionEvent = await _context.ConjunctionEvents
                .Include(c => c.PrimaryObject)
                .Include(c => c.SecondaryObject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conjunctionEvent == null)
            {
                return NotFound();
            }

            return View(conjunctionEvent);
        }

        // GET: Admin/ConjunctionEvent/Create
        public IActionResult Create()
        {
            ViewData["PrimaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType");
            ViewData["SecondaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType");
            return View();
        }

        // POST: Admin/ConjunctionEvent/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrimaryObjectId,SecondaryObjectId,TimeOfClosestApproach,MissDistance,CollisionProbability,Status,RiskAssessmentId,Id")] ConjunctionEvent conjunctionEvent)
        {
            if (ModelState.IsValid)
            {
                conjunctionEvent.Id = Guid.NewGuid();
                _context.Add(conjunctionEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PrimaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", conjunctionEvent.PrimaryObjectId);
            ViewData["SecondaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", conjunctionEvent.SecondaryObjectId);
            return View(conjunctionEvent);
        }

        // GET: Admin/ConjunctionEvent/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conjunctionEvent = await _context.ConjunctionEvents.FindAsync(id);
            if (conjunctionEvent == null)
            {
                return NotFound();
            }
            ViewData["PrimaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", conjunctionEvent.PrimaryObjectId);
            ViewData["SecondaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", conjunctionEvent.SecondaryObjectId);
            return View(conjunctionEvent);
        }

        // POST: Admin/ConjunctionEvent/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("PrimaryObjectId,SecondaryObjectId,TimeOfClosestApproach,MissDistance,CollisionProbability,Status,RiskAssessmentId,Id")] ConjunctionEvent conjunctionEvent)
        {
            if (id != conjunctionEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conjunctionEvent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConjunctionEventExists(conjunctionEvent.Id))
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
            ViewData["PrimaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", conjunctionEvent.PrimaryObjectId);
            ViewData["SecondaryObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", conjunctionEvent.SecondaryObjectId);
            return View(conjunctionEvent);
        }

        // GET: Admin/ConjunctionEvent/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conjunctionEvent = await _context.ConjunctionEvents
                .Include(c => c.PrimaryObject)
                .Include(c => c.SecondaryObject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (conjunctionEvent == null)
            {
                return NotFound();
            }

            return View(conjunctionEvent);
        }

        // POST: Admin/ConjunctionEvent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var conjunctionEvent = await _context.ConjunctionEvents.FindAsync(id);
            if (conjunctionEvent != null)
            {
                _context.ConjunctionEvents.Remove(conjunctionEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConjunctionEventExists(Guid id)
        {
            return _context.ConjunctionEvents.Any(e => e.Id == id);
        }
    }
}
