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
    public class SatelliteController : Controller
    {
        private readonly AppDbContext _context;

        public SatelliteController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Satellite
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Satellites.Include(s => s.Orbit).Include(s => s.Operator);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/Satellite/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var satellite = await _context.Satellites
                .Include(s => s.Orbit)
                .Include(s => s.Operator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (satellite == null)
            {
                return NotFound();
            }

            return View(satellite);
        }

        // GET: Admin/Satellite/Create
        public IActionResult Create()
        {
            ViewData["OrbitId"] = new SelectList(_context.Orbits, "Id", "Id");
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country");
            return View();
        }

        // POST: Admin/Satellite/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,LaunchDate,Status,OperatorId,NoradId,OrbitId,Id")] Satellite satellite)
        {
            if (ModelState.IsValid)
            {
                satellite.Id = Guid.NewGuid();
                _context.Add(satellite);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrbitId"] = new SelectList(_context.Orbits, "Id", "Id", satellite.OrbitId);
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country", satellite.OperatorId);
            return View(satellite);
        }

        // GET: Admin/Satellite/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var satellite = await _context.Satellites.FindAsync(id);
            if (satellite == null)
            {
                return NotFound();
            }
            ViewData["OrbitId"] = new SelectList(_context.Orbits, "Id", "Id", satellite.OrbitId);
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country", satellite.OperatorId);
            return View(satellite);
        }

        // POST: Admin/Satellite/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,LaunchDate,Status,OperatorId,NoradId,OrbitId,Id")] Satellite satellite)
        {
            if (id != satellite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(satellite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SatelliteExists(satellite.Id))
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
            ViewData["OrbitId"] = new SelectList(_context.Orbits, "Id", "Id", satellite.OrbitId);
            ViewData["OperatorId"] = new SelectList(_context.Operators, "Id", "Country", satellite.OperatorId);
            return View(satellite);
        }

        // GET: Admin/Satellite/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var satellite = await _context.Satellites
                .Include(s => s.Orbit)
                .Include(s => s.Operator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (satellite == null)
            {
                return NotFound();
            }

            return View(satellite);
        }

        // POST: Admin/Satellite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var satellite = await _context.Satellites.FindAsync(id);
            if (satellite != null)
            {
                _context.Satellites.Remove(satellite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SatelliteExists(Guid id)
        {
            return _context.Satellites.Any(e => e.Id == id);
        }
    }
}
