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


        //個股頁面
        public IActionResult StockCharts(string id) {
            ViewBag.stockID = id;
            return View();
        }

        //網址傳資料|從資料庫抓股票資料
        public IActionResult showStocks(string id) {

            var viewModel = _context.Stock.ToList();
            var query = (from p in viewModel
                         where p.SnCode == id
                         //先顛倒順序
                         orderby p.StDate descending
                         select new {
                             Date = p.StDate,
                             Open = p.SteOpen,
                             Close = p.SteClose,
                             High = p.SteMax,
                             Low = p.SteMin,
                             Volume = p.SteTradeQuantity,
                             SMA05 = p.SiMovingAverage5,
                             //MADC
                             Dif = p.SiDif,
                             Macd = p.SiMacd,
                             Osc = p.SiOsc,
                             //KD線
                             K05 = p.SiK5,
                             D05 = p.SiD5,
                             //
                             StockName = p.SnName,
                             SpreadRatio = p.SteSpreadRatio,
                             TradeMoney = p.SteTradeMoney,
                             TradeQuantity = p.SteTradeQuantity,
                             EPS = p.SbEps,
                             BussinessIncome = p.SbBussinessIncome,
                             //NonBussinessIncome = p.SbNonBussinessIncome,
                             //NonBussinessIncomeRatio = p.SbNonBussinessIncomeRatio

                         }).Take(100);   //取前一百筆
            //->這樣表會顛倒啊朋友

            //記得顛倒回來
            return Json(query.Reverse());
        }

        //網址傳資料|回傳預測內容
        public IActionResult showPredictions(string id) {
            var viewModel = _context.DbModels.ToList();
            var query = from p in viewModel
                        where p.Pstock == id
                        select new {
                            Account = p.Paccount,
                            Variable = p.Pvariable,
                            Label = p.Plabel,
                            FinishTime = Convert.ToDateTime(p.PfinishTime).ToString("yyyy-MM-dd")
                        };
            return Json(query);
        }

        //網址傳資料|該股票所有內容(for預測用
        public IActionResult showAllStocks(string id) {
            var viewModel = _context.Stock.ToList();
            var query = from p in viewModel
                        where p.SnCode == id
                        select new {
                            Date = p.StDate,
                            Close = p.SteClose,
                            StockName = p.SnName
                        };

            return Json(query);
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
