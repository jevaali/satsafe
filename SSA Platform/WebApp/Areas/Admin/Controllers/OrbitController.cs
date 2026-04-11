using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Domain.Entities;
using App.Infrastructure.Persistence;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrbitController : Controller
    {
        private readonly AppDbContext _context;

        public OrbitController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Orbit
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orbits.ToListAsync());
        }

        // GET: Admin/Orbit/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orbit = await _context.Orbits
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orbit == null)
            {
                return NotFound();
            }

            return View(orbit);
        }

        // GET: Admin/Orbit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Orbit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SemiMajorAxis,Eccentricity,Inclination,RAAN,ArgumentOfPerigee,MeanAnomaly,Epoch,SpaceObjectId,Id")] Orbit orbit)
        {
            if (ModelState.IsValid)
            {
                orbit.Id = Guid.NewGuid();
                _context.Add(orbit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orbit);
        }

        // GET: Admin/Orbit/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orbit = await _context.Orbits.FindAsync(id);
            if (orbit == null)
            {
                return NotFound();
            }
            return View(orbit);
        }

        // POST: Admin/Orbit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("SemiMajorAxis,Eccentricity,Inclination,RAAN,ArgumentOfPerigee,MeanAnomaly,Epoch,SpaceObjectId,Id")] Orbit orbit)
        {
            if (id != orbit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orbit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrbitExists(orbit.Id))
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
            return View(orbit);
        }

        // GET: Admin/Orbit/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orbit = await _context.Orbits
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orbit == null)
            {
                return NotFound();
            }

            return View(orbit);
        }

        // POST: Admin/Orbit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var orbit = await _context.Orbits.FindAsync(id);
            if (orbit != null)
            {
                _context.Orbits.Remove(orbit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrbitExists(Guid id)
        {
            return _context.Orbits.Any(e => e.Id == id);
        }
    }
}
