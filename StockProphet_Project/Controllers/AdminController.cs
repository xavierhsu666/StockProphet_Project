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
    }
}
