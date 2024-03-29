using Microsoft.AspNetCore.Mvc;
using StockProphet_Project.Models;
using System.Diagnostics;
using ChoETL;
using Microsoft.VisualBasic;
using Tensorflow;

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
            var myDateNow = DateOnly.FromDateTime(DateTime.Now);    //���դ�O03/22
            var myDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
            var viewModel = _context.Stock.ToList();
            var query =  from p in viewModel
                         where p.SnCode == id && p.StDate <= myDateNow && p.StDate >= myDate
                         //���A�˶���
                         //orderby p.StDate descending
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

                         };   //���e�@�ʵ�
            //->�o�˪�|�A�˰ڪB��
            Console.WriteLine(query);
            //�O�o�A�˦^��
            return Json(query);
        }

        //���}�Ǹ��|�^�ǹw�����e
          public IActionResult showPredictions(string id) {
              var viewModel = _context.DbModels.ToList();
            var viewModel2 = _context.DbCollect.ToList();

            var query = from p in viewModel
                        join c in viewModel2 on p.Pid equals c.PID into numCount
                        where p.Pstock == id
                        select new {
                            Account = p.Paccount,
                            Dummyblock = p.Dummyblock,  //���G�Ѽ�
                            Label = p.Plabel,
                            FinishTime = Convert.ToDateTime(p.PfinishTime).ToString("yyyy-MM-dd"),
                            PID = p.Pid,
                            BuildTime = Convert.ToDateTime(p.PbulidTime).ToString("yyyy-MM-dd"),
                            Variable = p.Pvariable,  //��ܪ��ܼ�
                            Status = p.Pstatus, //���A
                            NumCount = numCount.Count(), //���g��
                            Pmodel = p.Pmodel,   //�ϥμҫ�
                            PAR = p.PAccuracyRatio  //�ǽT�v
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

        //�[�J�ΧR���̷R�M��
        [HttpPost]
        public string CheckCard(string user, string cardID) {
            var member = _context.DbMembers.SingleOrDefault(e => e.MaccoMnt == user);
            Console.WriteLine("--------------"+user+"/" + cardID);
            char[] delimiterChars = ['{', '}', ','];
            string[] myCard = [];
            if ((member.MfavoriteModel) != null) { myCard = (member.MfavoriteModel).Split(delimiterChars); }
            //�����D������|���Ů�:(
            myCard = myCard.Where((source, index) => index != 0).ToArray();
            myCard = myCard.Where((source, index) => index != myCard.Length-1).ToArray();
            string change = "change?";   //�^�ǬO�R��(false)�B�s�W(true)�έ���(reject)
            var saveChange = "";


            if (member.MfavoriteModel == "{}" || member.MfavoriteModel == null) {   //�p�G���Ƭ���
                member.MfavoriteModel = "{" + cardID + "}";
                _context.SaveChanges();
                change = "A";
            } else  {    //��Ƥ�����
                    int i = Array.IndexOf(myCard, cardID);
                if (i > -1) {   //�C���o�ӭ�
                    myCard = myCard.Where((source, index) => index != i).ToArray();
                    saveChange = string.Join(",", myCard);  //array��z���r��
                    change = "D";
                } else if (i == -1) {   //�C��S�o�ӭ�
                    if (myCard.Length > 4) { //�w�W�L�����A�ڵ��s�W
                        saveChange = string.Join(",", myCard);
                        change = "R";
                    } else {
                        myCard = myCard.Concat(new string[] { cardID }).ToArray();
                        saveChange = string.Join(",", myCard);  //array��z���r��
                        change = "A";
                    }
                }
                member.MfavoriteModel = "{" + saveChange + "}";
                _context.SaveChanges();
            }
            string nowList = member.MfavoriteModel;

            return change+nowList;
        }

        //��n�J�̪��̷R�M��
        public IActionResult cardCheck(string id) {
            var member = _context.DbMembers.SingleOrDefault(e => e.MaccoMnt == id);
            char[] delimiterChars = ['{', '}', ','];
            string[] myCard = [];
            if ((member.MfavoriteModel) != null) { myCard = (member.MfavoriteModel).Split(delimiterChars); }
            myCard = myCard.Where((source, index) => index != 0).ToArray();
            myCard = myCard.Where((source, index) => index != myCard.Length - 1).ToArray();

            return Json(myCard);
        }


        //�ˬd�Ѳ��O�_�s�b
        public string checkStocks(string id) {
            var ans = "Nah";
            var stocksList = (from obj in new ChoCSVReader<stocksCheck>("wwwroot\\stocksList.csv").WithFirstLineHeader()
                              select obj).ToList();
            foreach (var stock in stocksList) {
                if (stock.Code == id || ((stock.Name).Split(' '))[0] == id || stock.Name == id) {
                    ans = stock.Code;
                    break;
                } else ans = "wrongCode";
            }
            return ans;
        }

        //�^�ǪѲ��W�٪��}�C��
        public IActionResult stocksListAC() {
            var stocksList = (from obj in new ChoCSVReader<stocksCheck>("wwwroot\\stocksList.csv").WithFirstLineHeader()
                              select new {
                                  label = obj.Name,
                                  category = obj.type
                              })
                              .ToList();

            return Json(stocksList);
        }


        public IActionResult Visitor() {
            return View();
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
