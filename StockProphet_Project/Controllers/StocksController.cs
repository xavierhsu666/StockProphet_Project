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
    public class StocksController : Controller
    {
        private readonly StocksContext _context;

        public StocksController(StocksContext context)
        {
            _context = context;
        }

		// GET: Stocks
		public async Task<IActionResult> Index(int? page)
		{
			// 每页显示的数据条数
			int pageSize = 10;

			// 查询数据总数
			var totalCount = await _context.Stock.CountAsync();

			// 计算总页数
			var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			// 当前页数，默认为第一页
			int pageNumber = page ?? 1;

			// 根据当前页数和每页显示的数据条数来计算要跳过的数据条数
			int skip = (pageNumber - 1) * pageSize;

			// 查询当前页需要显示的数据
			var stocks = await _context.Stock.OrderBy(x=>x.SnCode).ThenByDescending(x=>x.StDate).Skip(skip).Take(pageSize).ToListAsync();

			// 将分页信息传递给视图
			ViewData["TotalPages"] = totalPages;
			ViewData["CurrentPage"] = pageNumber;

			return View(stocks);
		}

		// GET: Stocks/Details/5
		public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stock
                .FirstOrDefaultAsync(m => m.SPk == id);
            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        // GET: Stocks/Create
        public IActionResult Create()
        {
            return View();
        }
        

        // POST: Stocks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SPk,StDate,StYearQuarter,StQuarter,StYear,SnCode,SnName,SteOpen,SteClose,SteMax,SteMin,SteSpreadRatio,SteTradeMoney,SteTradeQuantity,SteTransActions,SteDividendYear,SbYield,SbPbratio,SbEps,SbBussinessIncome,SiMovingAverage5,SiMovingAverage30,SiRsv5,SiRsv30,SiK5,SiK30,SiD5,SiD30,SiLongEma,SiShortEma,SiDif,SiMacd,SiOsc,SiPe,SiMa")] Stock stock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        // GET: Stocks/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }
            return View(stock);
        }

        // POST: Stocks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("SPk,StDate,StYearQuarter,StQuarter,StYear,SnCode,SnName,SteOpen,SteClose,SteMax,SteMin,SteSpreadRatio,SteTradeMoney,SteTradeQuantity,SteTransActions,SteDividendYear,SbYield,SbPbratio,SbEps,SbBussinessIncome,SiMovingAverage5,SiMovingAverage30,SiRsv5,SiRsv30,SiK5,SiK30,SiD5,SiD30,SiLongEma,SiShortEma,SiDif,SiMacd,SiOsc,SiPe,SiMa")] Stock stock)
        {
            if (id != stock.SPk)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockExists(stock.SPk))
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
            return View(stock);
        }

        // GET: Stocks/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stock
                .FirstOrDefaultAsync(m => m.SPk == id);
            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        // POST: Stocks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var stock = await _context.Stock.FindAsync(id);
            if (stock != null)
            {
                _context.Stock.Remove(stock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockExists(string id)
        {
            return _context.Stock.Any(e => e.SPk == id);
        }
    }
}
