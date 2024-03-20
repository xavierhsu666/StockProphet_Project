using Microsoft.AspNetCore.Mvc;
using StockProphet_Project.Models;

namespace StockProphet_Project.Controllers
{
    public class AdminController : Controller
    {
		private readonly StocksContext _context;
		public AdminController(StocksContext stockcontext)
		{
			_context = stockcontext;
		}
		public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AdminStock()
        {
            var query = _context.Stock
                .OrderBy(x=>x.SnCode)
                .ThenByDescending(x=> x.StDate ).ToList();

            return Json(query);
        }
        public IActionResult AdminMember()
        {
            var query=_context.DbMembers.ToList();
            return Json(query);
        }
        public IActionResult AdminModel()
        {
            var query = _context.DbModels.ToList();
            return Json(query);

        }
    }
}
