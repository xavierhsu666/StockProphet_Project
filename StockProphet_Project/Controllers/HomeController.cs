using Microsoft.AspNetCore.Mvc;
using StockProphet_Project.Models;
using System.Diagnostics;
using ChoETL;

namespace StockProphet_Project.Controllers {
    public class HomeController : Controller {
        private readonly StocksContext _context;
        public HomeController(StocksContext context) {
            _context = context;
        }

        public IActionResult Index() {
            return View();


        }

        //檢查股票是否存在
        public string checkStocks(string id) {
            var ans = "Nah";
            var stocksList = (from obj in new ChoCSVReader<stocksCheck>("wwwroot\\stocksList.csv").WithFirstLineHeader()
                              select obj).ToList();
            //if (stocksList.Any(n => n.Code == id || n.Name == id)) {
            //    ans = n.Name;
            //}
            foreach (var stock in stocksList) {
                if (stock.Code == id || stock.Name == id) {
                    ans = stock.Code;
                }
            }
            return ans;
        }

        //回傳股票名稱的陣列表
        public IActionResult stocksListAC() {
            var stocksList = (from obj in new ChoCSVReader<stocksCheck>("wwwroot\\stocksList.csv").WithFirstLineHeader()
                              select new {
                                  label = obj.Name,
                                  category = obj.type
                              })
                              .ToList();

            return Json(stocksList);
        }




        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
