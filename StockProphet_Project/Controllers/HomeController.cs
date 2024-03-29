using Microsoft.AspNetCore.Mvc;
using StockProphet_Project.Models;
using System.Diagnostics;
using ChoETL;
using Microsoft.VisualBasic;
using Tensorflow;

namespace StockProphet_Project.Controllers {
	public class HomeController : Controller {
		private readonly StocksContext _context;
		public HomeController( StocksContext context ) {
			_context = context;
		}

		public IActionResult Index() {
			return View();
		}


		//個股頁面
		public IActionResult StockCharts( string id ) {
			ViewBag.stockID = id;
			return View();
		}


		//網址傳資料|從資料庫抓股票資料
		public IActionResult showStocks( string id ) {
			var myDateNow = DateOnly.FromDateTime(DateTime.Now);    //測試日是03/22
			var myDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
			var viewModel = _context.Stock.ToList();
			var query = from p in viewModel
						where p.SnCode == id && p.StDate <= myDateNow && p.StDate >= myDate
						//先顛倒順序
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

						};   //取前一百筆
							 //->這樣表會顛倒啊朋友
			Console.WriteLine(query);
			//記得顛倒回來
			return Json(query);
		}

		//網址傳資料|回傳預測內容
		public IActionResult showPredictions( string id ) {
			var viewModel = _context.DbModels.ToList();
			var viewModel2 = _context.DbCollect.ToList();

			var query = from p in viewModel
						join c in viewModel2 on p.Pid equals c.PID into numCount
						where p.Pstock == id
						select new {
							Account = p.Paccount,
							Dummyblock = p.Dummyblock,  //成果參數
							Label = p.Plabel,
							FinishTime = Convert.ToDateTime(p.PfinishTime).ToString("yyyy-MM-dd"),
							PID = p.Pid,
							BuildTime = Convert.ToDateTime(p.PbulidTime).ToString("yyyy-MM-dd"),
							Variable = p.Pvariable,  //選擇的變數
							Status = p.Pstatus, //狀態
							NumCount = numCount.Count(), //按讚數
							Pmodel = p.Pmodel,   //使用模型
							PAR = p.PAccuracyRatio  //準確率
						};
			return Json(query);
		}




		//網址傳資料|該股票所有內容(for預測用
		public IActionResult showAllStocks( string id ) {
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
		// 請搜尋嘉澤做的Save to Collect DB
		public void SaveDataToCollect( string user, string cardID, string AorD ) {
			// test save http://localhost:5271/home/savedatatocollect?user=t1&cardID=1&AorD=A
			// test save http://localhost:5271/home/savedatatocollect?user=t1&cardID=2&AorD=A
			// test save http://localhost:5271/home/savedatatocollect?user=t1&cardID=3&AorD=A
			// test delete http://localhost:5271/home/savedatatocollect?user=t1&cardID=2&AorD=D
			Console.WriteLine(user + " " + cardID+" "+AorD);
			switch (AorD) {
				case "A":
					var newData = new DbCollect() {
						CID = user + "_" + cardID,
						PID = Convert.ToInt32(cardID),
						CDate = DateTime.Now,
						CAccount = user
					};
					_context.Add(newData);
					_context.SaveChanges();
					break;
				case "D":
					var recordToDelete = _context.DbCollect.FirstOrDefault(r => r.CID == user + "_" + cardID);
					if (recordToDelete != null) {
						_context.DbCollect.Remove(recordToDelete);
						_context.SaveChanges();
					}
					break;
			}
			//Console.WriteLine($"{user} {cardID}");
			
		}
		//加入或刪除最愛清單
		[HttpPost]
		public string CheckCard( string user, string cardID ) {


			var member = _context.DbMembers.SingleOrDefault(e => e.MaccoMnt == user);
			Console.WriteLine("--------------" + user + "/" + cardID);
			char[] delimiterChars = ['{', '}', ','];
			string[] myCard = [];
			if ((member.MfavoriteModel) != null) { myCard = (member.MfavoriteModel).Split(delimiterChars); }
			//不知道為什麼會有空格:(
			myCard = myCard.Where(( source, index ) => index != 0).ToArray();
			myCard = myCard.Where(( source, index ) => index != myCard.Length - 1).ToArray();
			string change = "change?";   //回傳是刪除(false)、新增(true)或限制(reject)
			var saveChange = "";
			

			if (member.MfavoriteModel == "{}" || member.MfavoriteModel == null) {   //如果表資料為空
				member.MfavoriteModel = "{" + cardID + "}";
				_context.SaveChanges();
				change = "A";

				SaveDataToCollect(user, cardID, change);
			} else {    //資料不為空
				int i = Array.IndexOf(myCard, cardID);
				if (i > -1) {   //列表有這個值
					myCard = myCard.Where(( source, index ) => index != i).ToArray();
					saveChange = string.Join(",", myCard);  //array整理成字串
					change = "D";
					SaveDataToCollect(user, cardID, change);
				} else if (i == -1) {   //列表沒這個值
					if (myCard.Length > 4) { //已超過五筆，拒絕新增
						saveChange = string.Join(",", myCard);
						change = "R";
					} else {
						myCard = myCard.Concat(new string[] { cardID }).ToArray();
						saveChange = string.Join(",", myCard);  //array整理成字串
						change = "A";
						SaveDataToCollect(user, cardID, change);
					}
				}
				member.MfavoriteModel = "{" + saveChange + "}";
				_context.SaveChanges();
			}
			string nowList = member.MfavoriteModel;

			return change + nowList;
		}

		//抓登入者的最愛清單
		public IActionResult cardCheck( string id ) {
			var member = _context.DbMembers.SingleOrDefault(e => e.MaccoMnt == id);
			char[] delimiterChars = ['{', '}', ','];
			string[] myCard = [];
			if ((member.MfavoriteModel) != null) { myCard = (member.MfavoriteModel).Split(delimiterChars); }
			myCard = myCard.Where(( source, index ) => index != 0).ToArray();
			myCard = myCard.Where(( source, index ) => index != myCard.Length - 1).ToArray();

			return Json(myCard);
		}


		//檢查股票是否存在
		public string checkStocks( string id ) {
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
