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


        //�Ӫѭ���
        public IActionResult StockCharts(string id) {
            ViewBag.stockID = id;
            return View();
        }

        //���}�Ǹ��|�q��Ʈw��Ѳ����
        public IActionResult showStocks(string id) {

            var viewModel = _context.Stock.ToList();
            var query = (from p in viewModel
                         where p.SnCode == id
                         //���A�˶���
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
                             //KD�u
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

                         }).Take(100);   //���e�@�ʵ�
            //->�o�˪�|�A�˰ڪB��

            //�O�o�A�˦^��
            return Json(query.Reverse());
        }

        //���}�Ǹ��|�^�ǹw�����e
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

        //���}�Ǹ��|�ӪѲ��Ҧ����e(for�w����
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
