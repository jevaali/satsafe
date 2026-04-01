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
    public class TleRecordController : Controller
    {
        private readonly AppDbContext _context;

        public TleRecordController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/TleRecord
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.TleRecords.Include(t => t.SpaceObject);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/TleRecord/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tleRecord = await _context.TleRecords
                .Include(t => t.SpaceObject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tleRecord == null)
            {
                return NotFound();
            }

            return View(tleRecord);
        }

        // GET: Admin/TleRecord/Create
        public IActionResult Create()
        {
            ViewData["SpaceObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType");
            return View();
        }

        // POST: Admin/TleRecord/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpaceObjectId,Epoch,Inclination,RAAN,Eccentricity,ArgumentOfPerigee,MeanAnomaly,MeanMotion,RevolutionNumber,BSTar,FirstDerivative,SecondDerivative,RawLine1,RawLine2,Id")] TleRecord tleRecord)
        {
            if (ModelState.IsValid)
            {
                tleRecord.Id = Guid.NewGuid();
                _context.Add(tleRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SpaceObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", tleRecord.SpaceObjectId);
            return View(tleRecord);
        }

        // GET: Admin/TleRecord/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tleRecord = await _context.TleRecords.FindAsync(id);
            if (tleRecord == null)
            {
                return NotFound();
            }
            ViewData["SpaceObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", tleRecord.SpaceObjectId);
            return View(tleRecord);
        }

        // POST: Admin/TleRecord/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("SpaceObjectId,Epoch,Inclination,RAAN,Eccentricity,ArgumentOfPerigee,MeanAnomaly,MeanMotion,RevolutionNumber,BSTar,FirstDerivative,SecondDerivative,RawLine1,RawLine2,Id")] TleRecord tleRecord)
        {
            if (id != tleRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tleRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TleRecordExists(tleRecord.Id))
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
            ViewData["SpaceObjectId"] = new SelectList(_context.Set<SpaceObject>(), "Id", "ObjectType", tleRecord.SpaceObjectId);
            return View(tleRecord);
        }

        // GET: Admin/TleRecord/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tleRecord = await _context.TleRecords
                .Include(t => t.SpaceObject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tleRecord == null)
            {
                return NotFound();
            }

            return View(tleRecord);
        }

        // POST: Admin/TleRecord/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tleRecord = await _context.TleRecords.FindAsync(id);
            if (tleRecord != null)
            {
                _context.TleRecords.Remove(tleRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TleRecordExists(Guid id)
        {
            return _context.TleRecords.Any(e => e.Id == id);
        }
    }
}
