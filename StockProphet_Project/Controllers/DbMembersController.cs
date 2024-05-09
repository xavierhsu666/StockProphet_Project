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
    public class DbMembersController : Controller
    {
        private readonly StocksContext _context;

        public DbMembersController(StocksContext context)
        {
            _context = context;
        }

        // GET: DbMembers
        public async Task<IActionResult> Index()
        {
            return View(await _context.DbMembers.ToListAsync());
        }

        // GET: DbMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbMember = await _context.DbMembers
                .FirstOrDefaultAsync(m => m.Mid == id);
            if (dbMember == null)
            {
                return NotFound();
            }

            return View(dbMember);
        }

        // GET: DbMembers/Create
        public IActionResult Create()
        {
            return View();
        }
		[HttpGet]
		public IActionResult selectcustomername(string data)
		{
			var query = _context.DbMembers.Where(x => x.MtrueName == data);
			return Json(query);

		}
		[HttpGet]
		public IActionResult piechart()
		{
			var query = _context.DbMembers;
			return Json(query);

		}

		// POST: DbMembers/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Mid,MaccoMnt,Mpassword,MtrueName,Mgender,Mbirthday,MinvestYear,Memail,Mlevel,Mprefer,MregisterTime,MlastLoginTime,MfavoriteModel")] DbMember dbMember)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dbMember);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dbMember);
        }

        // GET: DbMembers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbMember = await _context.DbMembers.FindAsync(id);
            if (dbMember == null)
            {
                return NotFound();
            }
            return View(dbMember);
        }

        // POST: DbMembers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Mid,MaccoMnt,Mpassword,MtrueName,Mgender,Mbirthday,MinvestYear,Memail,Mlevel,Mprefer,MregisterTime,MlastLoginTime,MfavoriteModel")] DbMember dbMember)
        {
            if (id != dbMember.Mid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DbMemberExists(dbMember.Mid))
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
            return View(dbMember);
        }

        // GET: DbMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbMember = await _context.DbMembers
                .FirstOrDefaultAsync(m => m.Mid == id);
            if (dbMember == null)
            {
                return NotFound();
            }

            return View(dbMember);
        }

        // POST: DbMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dbMember = await _context.DbMembers.FindAsync(id);
            if (dbMember != null)
            {
                _context.DbMembers.Remove(dbMember);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DbMemberExists(int id)
        {
            return _context.DbMembers.Any(e => e.Mid == id);
        }
    }
}
