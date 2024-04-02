using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using StockProphet_Project.Models;

namespace StockProphet_Project.Controllers
{
    public class DbCollectsController : Controller
    {
        private readonly StocksContext _context;

        public DbCollectsController(StocksContext context)
        {
            _context = context;
        }
        [HttpGet]
		public  IActionResult Top5select()
		{
			var top5Accounts = _context.DbCollect
		                .GroupBy(c => c.PID)
		                .Select(g => new
		                {
			                PID = g.Key,
			                CAccountCount = g.Count()
		                })
		                .OrderByDescending(x => x.CAccountCount)
		                .Take(5)
		                .ToList();


			return Json(top5Accounts);
		}
		[HttpGet]
		public IActionResult Top5selecttable(int data)
		{

            var top5data = _context.DbModels.Where(x => x.Pid == data).ToList();

			


			return Json(top5data);
		}

		// GET: DbCollects
		public async Task<IActionResult> Index()
        {
            return View(await _context.DbCollect.ToListAsync());
        }

        // GET: DbCollects/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbCollect = await _context.DbCollect
                .FirstOrDefaultAsync(m => m.CID == id);
            if (dbCollect == null)
            {
                return NotFound();
            }

            return View(dbCollect);
        }

        // GET: DbCollects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DbCollects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CID,PID,CAccount,CDate")] DbCollect dbCollect)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dbCollect);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dbCollect);
        }

        // GET: DbCollects/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbCollect = await _context.DbCollect.FindAsync(id);
            if (dbCollect == null)
            {
                return NotFound();
            }
            return View(dbCollect);
        }

        // POST: DbCollects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CID,PID,CAccount,CDate")] DbCollect dbCollect)
        {
            if (id != dbCollect.CID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbCollect);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DbCollectExists(dbCollect.CID))
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
            return View(dbCollect);
        }

        // GET: DbCollects/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbCollect = await _context.DbCollect
                .FirstOrDefaultAsync(m => m.CID == id);
            if (dbCollect == null)
            {
                return NotFound();
            }

            return View(dbCollect);
        }

        // POST: DbCollects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var dbCollect = await _context.DbCollect.FindAsync(id);
            if (dbCollect != null)
            {
                _context.DbCollect.Remove(dbCollect);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DbCollectExists(string id)
        {
            return _context.DbCollect.Any(e => e.CID == id);
        }

    }
}
