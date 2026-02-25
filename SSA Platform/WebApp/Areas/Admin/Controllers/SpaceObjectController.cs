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
    public class SpaceObjectController : Controller
    {
        private readonly AppDbContext _context;

        public SpaceObjectController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/SpaceObject
        public async Task<IActionResult> Index()
        {
            return View(await _context.SpaceObjects.ToListAsync());
        }

        // GET: Admin/SpaceObject/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spaceObject = await _context.SpaceObjects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (spaceObject == null)
            {
                return NotFound();
            }

            return View(spaceObject);
        }

        // GET: Admin/SpaceObject/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/SpaceObject/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoradId,OrbitId,Id")] SpaceObject spaceObject)
        {
            if (ModelState.IsValid)
            {
                spaceObject.Id = Guid.NewGuid();
                _context.Add(spaceObject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(spaceObject);
        }

        // GET: Admin/SpaceObject/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spaceObject = await _context.SpaceObjects.FindAsync(id);
            if (spaceObject == null)
            {
                return NotFound();
            }
            return View(spaceObject);
        }

        // POST: Admin/SpaceObject/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("NoradId,OrbitId,Id")] SpaceObject spaceObject)
        {
            if (id != spaceObject.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spaceObject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpaceObjectExists(spaceObject.Id))
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
            return View(spaceObject);
        }

        // GET: Admin/SpaceObject/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spaceObject = await _context.SpaceObjects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (spaceObject == null)
            {
                return NotFound();
            }

            return View(spaceObject);
        }

        // POST: Admin/SpaceObject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var spaceObject = await _context.SpaceObjects.FindAsync(id);
            if (spaceObject != null)
            {
                _context.SpaceObjects.Remove(spaceObject);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpaceObjectExists(Guid id)
        {
            return _context.SpaceObjects.Any(e => e.Id == id);
        }
    }
}
