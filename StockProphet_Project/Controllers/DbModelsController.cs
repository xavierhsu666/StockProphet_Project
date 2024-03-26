using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockProphet_Project.Models;

namespace StockProphet_Project.Controllers
{
    public class DbModelsController : Controller
    {
        private readonly StocksContext _context;

        public DbModelsController(StocksContext context)
        {
            _context = context;
        }

        // GET: DbModels
        public async Task<IActionResult> Index()
        {
            return View(await _context.DbModels.ToListAsync());
        }

        // GET: DbModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbModel = await _context.DbModels
                .FirstOrDefaultAsync(m => m.Pid == id);
            if (dbModel == null)
            {
                return NotFound();
            }

            return View(dbModel);
        }

        // GET: DbModels/Create
        public IActionResult Create()
        {
            return View();
        }
		public IActionResult orderdata(string data)
		{
			var query = _context.DbModels.Where(x => x.Pstock == data);
            return Json(query);
		}
		public IActionResult barchart()
		{
            var query = _context.DbModels;
			return Json(query);
		}

		// POST: DbModels/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Pid,Pstock,Pvariable,Plabel,Pprefer,Paccount,PbulidTime,PfinishTime,Pstatus,Dummyblock")] DbModel dbModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dbModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dbModel);
        }

        // GET: DbModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbModel = await _context.DbModels.FindAsync(id);
            if (dbModel == null)
            {
                return NotFound();
            }
            return View(dbModel);
        }

        // POST: DbModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Pid,Pstock,Pvariable,Plabel,Pprefer,Paccount,PbulidTime,PfinishTime,Pstatus,Dummyblock")] DbModel dbModel)
        {
            if (id != dbModel.Pid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DbModelExists(dbModel.Pid))
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
            return View(dbModel);
        }

        // GET: DbModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbModel = await _context.DbModels
                .FirstOrDefaultAsync(m => m.Pid == id);
            if (dbModel == null)
            {
                return NotFound();
            }

            return View(dbModel);
        }

        // POST: DbModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dbModel = await _context.DbModels.FindAsync(id);
            if (dbModel != null)
            {
                _context.DbModels.Remove(dbModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DbModelExists(int id)
        {
            return _context.DbModels.Any(e => e.Pid == id);
        }
    }
}
