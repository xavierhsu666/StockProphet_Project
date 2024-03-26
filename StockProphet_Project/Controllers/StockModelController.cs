using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using Tensorflow.NumPy;
using static Tensorflow.KerasApi;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Engine;
using Tensorflow;
using Tensorflow.Train;
using static Tensorflow.ApiDef.Types;
using System.Security.Cryptography.Xml;
using StockProphet_Project.Models;
using System.Diagnostics;
using Serilog;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Tensorflow.NumPy.Pickle;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;
using Tensorflow.Keras.Callbacks;
using Tensorflow.Keras;
using Tensorflow.Keras.Layers;
using Tensorflow.Keras.Models;
using Tensorflow.Keras.ArgsDefinition;
using Tensorflow.Keras.Datasets;
using Tensorflow.Operations.Activation;
using Microsoft.AspNetCore.Cors;
using System.Text;
using static StockProphet_Project.Models.WebAPI_Class;
using ChoETL;
using System.Security.Cryptography.X509Certificates;


namespace StockProphet_Project.Controllers {
	public class StockModelController : Controller {
		private readonly StocksContext _context;
		private readonly ILogger<HomeController> _logger;

		public StockModelController( ILogger<HomeController> logger, StocksContext context ) {
			_logger = logger;
			_context = context;
		}

		// ------------------------------------------------------------------------------<KAZUO>------------------------------------------------------------------------------

		// <參數區>改過需要調整的地方
		public string InputLatestDate = DateTime.Parse("2024-3-8").ToString("yyyy-MM-dd");
		public int InputMinCount = 30;
		// <參數區>改過需要調整的地方

		// <view頁面區>
		//public IActionResult TestWsWepAPI() {
		//	CallPyApi cpa = new CallPyApi();
		//	var fedback = cpa.UpdateOneStock("8271", InputLatestDate.Replace("-", ""));

		//	return Content(fedback);

		//}
		public async Task<IActionResult> updateAllStock() {
			var query = _context.Stock
				   .AsEnumerable()
				   .GroupBy(o => o.SnCode)
				   .Select(g => g.First());
			string log;
			log = "KS: Start Update all stock.\r\n";
			int i = 0;
			foreach (var item in query) {
				CallPyApi cpa = new CallPyApi();
				Console.WriteLine("KS: 呼叫UpdateOneStock(stockCode=" + item.SnCode + ")");
				log += "_________________ " + i + " \"_________________ \"\r\n";
				log += "KS: 呼叫UpdateOneStock(stockCode=" + item.SnCode + ") for "+i+" Time .\r\n";
				string fedback = await cpa.UpdateOneStock(item.SnCode, InputLatestDate.Replace("-", ""));
				log += "KS: Finished.\r\n";
				i++;
			}
			return Content(log);

		}

		public async Task<IActionResult> TestWepAPI() {
			WebAPI_Class wac = new WebAPI_Class();
			wac.stockCode = "2330";
			wac.date = DateTime.Now.AddDays(-10);
			wac.stockName = "台積電";
			//List<Table3OBJ> result = await wac.ajax_3();
			//List<Table1OBJ> result = await wac.ajax_1();
			//List<Table2OBJ> result = await wac.ajax_2();
			wac.run();

			////// 使用 result 中的元素
			//foreach (var item in result) {
			//	Console.WriteLine($"Stock Code: {item.date}, " +
			//		$"Stock date: {item.SB_PBRatio}, " +
			//		$"Stock OpenPrice: {item.SB_Yield}, " +
			//		$"Stock MaxPrice: {item.ST_Year_Quarter}, " +
			//		$"Stock MinPrice: {item.STe_Dividend_Year}, " +
			//		$"Stock ClosePrice: {item.SI_PE}"
			//		);
			//}


			return Content("OK");
		}
		[EnableCors("MyAllowSpecificOrigins")]
		[HttpGet]
		public async Task<IActionResult> test1() {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			using (HttpClient client = new HttpClient()) {
				try {
					// 設定 Web API 的 URL
					string apiUrl = "https://mops.twse.com.tw/nas/t21/sii/t21sc03_113_2_0.html";

					// 發送 GET 請求並等待回應
					HttpResponseMessage response = await client.GetAsync(apiUrl);

					// 確認請求成功
					if (response.IsSuccessStatusCode) {
						// 讀取回傳的資料
						byte[] bytes = await response.Content.ReadAsByteArrayAsync();
						// 將資料轉換為 UTF-8 編碼
						string responseBody = Encoding.GetEncoding("big5").GetString(bytes);

						// 將資料以 UTF-8 編碼返回

						Console.WriteLine("API 回傳的資料：");
						//Console.WriteLine(responseBody);
						return Content(responseBody, "text/html", Encoding.GetEncoding("big5"));
					} else {
						Console.WriteLine($"請求失敗，狀態碼：{response.StatusCode}");
						return Content($"請求失敗，狀態碼：{response.StatusCode}");
					}
				} catch (HttpRequestException e) {
					Console.WriteLine($"發生 HTTP 錯誤： {e.Message}");
					return Content($"發生 HTTP 錯誤： {e.Message}");
				}
			}
		}
		public IActionResult TestBuild() {// 获取表单中的所有输入值
			return View();
		}
		public IActionResult Build( [FromForm] IFormCollection form ) {// 获取表单中的所有输入值

			// 先看一下DB內的資料是否最新

			var q = from o in _context.Stock
					where o.StDate <= DateOnly.Parse(InputLatestDate) &&
						  o.SnCode == form["stockCode"].ToString()
					select o;
			string log = "";
			if (q.Count() < InputMinCount)
				return View("index");

			// 根據選擇近到不同function
			switch (form["modelPick"].ToString()) {
				case "regression":
					log = "Regression Building...";
					// 建立model input 的參數
					string[] inputVar = new string[] { "STe_Open", "STe_Close", "STe_Max", "STe_Min", "STe_SpreadRatio" };
					KsModelClass.ModelInput mi = new KsModelClass.ModelInput() {
						inputPara = inputVar,
						maximumNumberOfIterations = 100
					};

					var mo = RegessionBuild(q, mi);
					//mo.output_F_Forcast,mo.output_M_RMSE,mo.output_M_MSE
					ViewBag.result = new double[] { mo.output_F_Forcast, mo.output_M_RMSE, mo.output_M_MSE };
					log += "Regression Builded. ";

					break;
				case "TimeSerial":
					log = "TimeSerial Building...";
					TimeSerialModel.ModelInput mp = new TimeSerialModel.ModelInput() {
						windowSize = 7,
						seriesLength = 30,
						trainSize = 365,
						confidenceLevel = 0.95f,
						focastDate = DateTime.Parse(InputLatestDate)

					};

					var tmo = TimeSerialBuild(q, mp);
					ViewBag.result = new double[] {
						tmo.Output_F_estimate ,
						tmo.Output_M_RMSE,
						tmo.Output_M_MAE,
						tmo.Output_F_upperEstimate,
						tmo.Output_F_lowerEstimate
					};

					log += "TimeSerial Builded. ";


					break;
				default:
					log = "failed";
					break;
			}

			// 最新的話
			// 抓要建什麼模型分case去建
			// 不是的話
			// 退回去讓他在呼一次API
			ViewBag.ModelInputConut = q.Count();
			ViewBag.log = log;
			return View(form);
		}
		// <view頁面區>
		// <功能開發測試區>
		public string checkStockCodeIsRight( string id ) {
			var ans = "Nah";
			var stocksList = (from obj in new ChoCSVReader<stocksCheck>("wwwroot\\stocksList.csv").WithFirstLineHeader()
							  select obj).ToList();
			foreach (var stock in stocksList) {
				if (stock.Code == id || stock.Name == id) {
					ans = stock.Code;
				}
			}
			return ans;
		}
		public TimeSerialModel.ModelOutput TimeSerialBuild( IQueryable<Stock> q, TimeSerialModel.ModelInput mp ) {// 获取表单中的所有输入值
			TimeSerialModel t = new TimeSerialModel();

			var ModelReturn = t.KsMLModeling(q.ToList(), mp);
			return ModelReturn;
		}

		public KsModelClass.ModelOutput RegessionBuild( IQueryable<Stock> q, KsModelClass.ModelInput mi ) {// 获取表单中的所有输入值
			KsModelClass mc = new KsModelClass();
			var a = mc.MLModeling(q.ToList(), mi);
			return a;
		}
		public List<string> TransferToColumnName( string[] input ) {
			List<string> output = new List<string>();

			var mapping = new[] { "S_PK", "ST_Date", "ST_Year_Quarter", "ST_Quarter", "ST_Year", "SN_Code", "SN_Name", "STe_Open", "STe_Close", "STe_Max", "STe_Min", "STe_SpreadRatio", "STe_TradeMoney", "STe_TradeQuantity", "STe_TransActions", "STe_Dividend_Year", "SB_Yield", "SB_PBRatio", "SB_EPS", "SB_BussinessIncome", "SI_MovingAverage_5", "SI_MovingAverage_30", "SI_RSV_5", "SI_RSV_30", "SI_K_5", "SI_K_30", "SI_D_5", "SI_D_30", "SI_LongEMA", "SI_ShortEMA", "SI_Dif", "SI_MACD", "SI_OSC", "SI_PE", "SI_MA" };
			var mapping2 = new[] { "SPk", "StDate", "StYearQuarter", "StQuarter", "StYear", "SnCode", "SnName", "SteOpen", "SteClose", "SteMax", "SteMin", "SteSpreadRatio", "SteTradeMoney", "SteTradeQuantity", "SteTransActions", "SteDividendYear", "SbYield", "SbPbratio", "SbEps", "SbBussinessIncome", "SiMovingAverage5", "SiMovingAverage30", "SiRsv5", "SiRsv30", "SiK5", "SiK30", "SiD5", "SiD30", "SiLongEma", "SiShortEma", "SiDif", "SiMacd", "SiOsc", "SiPe", "SiMa" };
			foreach (var key in input) {
				Console.WriteLine("TransferToColumnName");
				Console.WriteLine("_ index of = " + input[0].IndexOf("_"));
				var q = (from o in (input[0].IndexOf("_") >= 0) ? mapping2 : mapping
						 where o.Replace("_", "").ToLower() == key.Replace("_", "").ToLower()
						 select o).FirstOrDefault();
				Console.WriteLine("key = " + key);
				Console.WriteLine("search result = " + q);
				Console.WriteLine("_____________________________");
				output.add(q);
			}

			return output;
		}
		public bool checkStockData_Latest( string stockCode ) {
			var query = from o in _context.Stock
						where o.SnCode == stockCode
						orderby o.StDate descending
						select o;
			if (DateTime.Parse(query.FirstOrDefault().StDate.ToString()) == DateTime.Parse(InputLatestDate)) {
				Console.WriteLine(DateTime.Parse(query.FirstOrDefault().StDate.ToString()));
				Console.WriteLine(DateTime.Parse(InputLatestDate));
				return true;
			} else {
				Console.WriteLine(DateTime.Parse(query.FirstOrDefault().StDate.ToString()));
				Console.WriteLine(DateTime.Parse(InputLatestDate));
				return false;
			}

		}
		public double[] ModelOutputCheck( string stockCode, int userPrefer, float esti, float u = 0, float l = 0 ) {
			if (userPrefer == 1) {
				var q = from o in _context.Stock
						where o.SnCode == stockCode
						orderby o.StDate descending
						select o;
				var e = (float)(q.FirstOrDefault().SteClose);

				Console.WriteLine("Adjust the Usable price");
				Console.WriteLine("e = " + e);
				Console.WriteLine("esti = " + esti);
				Console.WriteLine("esti/e = " + esti / e);
				Console.WriteLine("u = " + u);
				Console.WriteLine("l = " + l);
				if (esti / e > 1.1 || esti / e < 0.9) {
					var spread = esti / e;
					if (spread > 1) {
						u = u - (esti - (e * (float)1.1));
						l = l - (esti - (e * (float)1.1));
						esti = e * (float)1.1;
					} else {
						u = u - (esti - (e * (float)0.9));
						l = l - (esti - (e * (float)0.9));
						esti = e * (float)0.9;
					}
					return new double[] { esti, u, l };
				} else {
					return new double[] { esti, u, l };
				}
			} else {
				return new double[] { esti, u, l };
			}
		}

		// <功能開發測試區>

		// <WEB API 區>
		public IActionResult stocksListACA() {
			var stocksList = (from obj in new ChoCSVReader<stocksCheck>("wwwroot\\stocksListCode.csv").WithFirstLineHeader()
							  select new {
								  label = obj.Name,
								  category = obj.type
							  })
							  .ToList();

			return Json(stocksList);
		}
		[HttpGet]
		public async Task<IActionResult> UpdateOneStock( string stockCode ) {
			if (checkStockCodeIsRight(stockCode) == "Nah") {
				var result = new { stockname = "查無這支股票", stockexist = false };
				return Json(result);
			} else {


				CallPyApi cpa = new CallPyApi();
				Console.WriteLine("KS: 呼叫UpdateOneStock(stockCode=" + stockCode + ")");
				string fedback = await cpa.UpdateOneStock(stockCode, InputLatestDate.Replace("-", ""));
				var stockData = _context.Stock
						.Where(x => x.SnCode == stockCode)
						.OrderBy(x => x.StDate)
						.ToList();
				//var stockData = from o in _context.Stock.ToList()
				//				where o.SnCode == stockCode
				//				select o;

				string stockname;
				bool stockexist;
				if (stockData == null || stockData.Count() == 0) {
					stockname = "查無這支股票";
					stockexist = false;
				} else {
					stockname = stockData.First().SnName;
					stockexist = true;
				}
				//stockname = "查無這支股票";
				//stockexist = false;
				var result = new { stockname = stockname, stockexist = stockexist };
				return Json(result);
			}
		}
		[HttpPost]
		public IActionResult getStock( APIClass apiData ) {

			var query = from o in _context.Stock
						where o.SnCode == apiData.stockCode
						select o;
			var returnVal = 0;
			var log = "";
			if (query.Count() <= 0) {
				// 如果資料庫沒資料，呼叫API撈進資料庫
				log = "資料庫沒資料";
			} else { //如果有，確認是否要更新資料庫
				var q3 = from o in query
							 //where o.ST_Date == DateTime.Now.ToString("yyyyMMdd")
						 where o.StDate == DateOnly.Parse(InputLatestDate)
						 select o;
				var q2 = from o in _context.Stock
						 where o.SnCode == apiData.stockCode &&
						 o.StDate == DateOnly.Parse(InputLatestDate)
						 select o;
				if (q2.Count() >= 1) { //資料是最新
					log = "資料庫有資料，且是最新";
					returnVal = q2.Count();
				} else { //資料不是最新
					log = "資料庫有資料，但不是最新";
					returnVal = q2.Count();

				}

			}
			var returnData = new {
				stockCode = apiData.stockCode,
				InputLatestDate = InputLatestDate,
				ThisStockCount = query.Count(),
				ThisStockLatestDataCount = returnVal,
				log = log,

			};
			return Json(returnData);
		}

		[HttpPost]
		//public IActionResult ModelInputPack( int CheckCode, string stockCode, string[] InputColumnName, string usingModel, int? R_maximumNumberOfIterations, int? T_windowSize, int? T_seriesLength, int? T_trainSize, int? T_confidenceLevel ) {
		public IActionResult ModelInputPack( WebInput wi ) {
			// 增加檢查碼、stockCode
			var data = new {
				stockCode = wi.stockCode,
				InputColumnName = wi.InputColumnName,
				R_maximumNumberOfIterations = wi.R_maximumNumberOfIterations ?? -1,
				T_windowSize = wi.T_windowSize ?? -1,
				T_seriesLength = wi.T_seriesLength ?? -1,
				T_trainSize = wi.T_trainSize ?? -1,
				T_confidenceLevel = wi.T_confidenceLevel ?? -1,
				usingModel = wi.usingModel,
				userPrefer = wi.userPrefer
			};
			// 摳算存

			var q = from o in _context.Stock
					where o.SnCode == wi.stockCode
					select o;

			switch (data.usingModel) {
				case "R":
					Console.WriteLine("Regression Building...............");

					// 建立model input 的參數
					//string[] inputVar = wi.InputColumnName;
					//new string[] { "STe_Open", "STe_Close", "STe_Max", "STe_Min", "STe_SpreadRatio" };
					string[] inputVar = TransferToColumnName(wi.InputColumnName).ToArray();
					KsModelClass.ModelInput mi = new KsModelClass.ModelInput() {
						inputPara = inputVar,
						maximumNumberOfIterations = (int)wi.R_maximumNumberOfIterations,
						userPrefer = (int)wi.userPrefer
					};

					var mo = RegessionBuild(q, mi);
					//mo.output_F_Forcast,mo.output_M_RMSE,mo.output_M_MSE
					var moo = ModelOutputCheck(wi.stockCode, (int)wi.userPrefer, mo.output_F_Forcast);
					var jmo = new {
						output_F_Forcast = moo[0],
						output_M_RMSE = mo.output_M_RMSE,
						output_M_MSE = mo.output_M_MSE
					};
					ViewBag.result = new double[] { mo.output_F_Forcast, mo.output_M_RMSE, mo.output_M_MSE };
					Console.WriteLine("Regression Builded................");
					return Json(jmo);
				case "T":
					Console.WriteLine("Regression Building...............");
					//Console.WriteLine("Check Input data is latest?...............");

					//if (!checkStockData_Latest(wi.stockCode))
					//	return View("predictIndex");
					Console.WriteLine("Check Input data OK...............");
					TimeSerialModel.ModelInput tmi = new TimeSerialModel.ModelInput() {
						focastDate = DateTime.Parse(InputLatestDate),
						confidenceLevel = (float)((float)data.T_confidenceLevel / (float)100),
						windowSize = data.T_windowSize,
						seriesLength = data.T_seriesLength,
						trainSize = data.T_trainSize
					};

					var tmo = TimeSerialBuild(q, tmi);
					var qoo = ModelOutputCheck(wi.stockCode, (int)wi.userPrefer, tmo.Output_F_estimate, tmo.Output_F_upperEstimate, tmo.Output_F_lowerEstimate);
					tmo.Output_F_estimate = (float)qoo[0];
					tmo.Output_F_upperEstimate = (float)qoo[1];
					tmo.Output_F_lowerEstimate = (float)qoo[2];
					ViewBag.result = new double[] {
						tmo.Output_M_MAE,
						tmo.Output_M_RMSE,
						qoo[0],
						qoo[2],
						qoo[1]
					};
					return Json(tmo);
				default:
					return Json(data);
					break;
			}


		}
		[HttpPost]
		public IActionResult SaveModelResult( ModelResult mr ) {
			var query = new StocksContext();
			DateTime buildTime = DateTime.Parse(mr.PBuildTime);
			DateTime finishTime = DateTime.Parse(mr.PfinishTime);
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine(mr.dummyblock);
			Console.WriteLine("---------------------------------------------");
			//System.Diagnostics.Debug.WriteLine($"PLabel: {PLabel}");
			var newdata = new DbModel {
				Pstock = mr.PStock,
				Pvariable = mr.PVariable,
				Plabel = mr.PLabel,
				Pprefer = mr.PPrefer,
				PbulidTime = buildTime,
				PfinishTime = finishTime,
				Dummyblock = mr.dummyblock,
				Paccount = mr.Paccount
			};
			//System.Diagnostics.Debug.WriteLine($"PbulidTime: {buildTime}");
			query.DbModels.Add(newdata);
			query.SaveChanges();
			var user = _context.DbMembers.FirstOrDefault(o => o.MaccoMnt == mr.Paccount);
			if (user != null) {
				// 更新會員相關的屬性
				user.Mprefer = mr.PPrefer; // 這裡請替換為您想要更新的屬性和值
				_context.DbMembers.Update(user);
				_context.SaveChanges();
			}
			return Json(newdata);
		}

		// <WEB API 區>
		// <類別區>
		public class APIClass {
			public string stockCode { get; set; }
		}
		public class WebInput {
			public string stockCode { get; set; }
			public string[] InputColumnName { get; set; }
			public int? R_maximumNumberOfIterations { get; set; }
			public int? T_windowSize { get; set; }
			public int? T_seriesLength { get; set; }
			public int? T_trainSize { get; set; }
			public int? T_confidenceLevel { get; set; }
			public string usingModel { get; set; }
			public int? userPrefer { get; set; }

		}
		public class ModelResult {
			public string PStock { get; set; }
			public string PVariable { get; set; }
			public decimal PLabel { get; set; }
			public byte PPrefer { get; set; }
			public string PBuildTime { get; set; }
			public string PfinishTime { get; set; }
			public string dummyblock { get; set; }
			public string Paccount { get; set; }
		}

		// <類別區>
		// ------------------------------------------------------------------------------<KAZUO>------------------------------------------------------------------------------





		// GET: Stockinfoes
		public async Task<IActionResult> Index() {
			return View(await _context.Stock.ToListAsync());
		}
		[HttpPost]
		public IActionResult LSTMpredict( string sncode, int predictday, Dictionary<string, bool> selectedParams, int iterationtime ) {
			//測試區
			// 在控制台輸出收到的資料以進行檢查
			//Console.WriteLine("Received data:");
			//Console.WriteLine("SN Code: " + sncode);	
			//Console.WriteLine("Predict Day: " + predictday);
			//Console.WriteLine("Selected Params:");
			//foreach (var key in selectedParams.Keys)
			//{
			//    Console.WriteLine(key);
			//}
			//return Ok("Some result");

			float predictionResult;
			int selsectedcount = selectedParams.Count;
			string predictionResulttoString;
			//撈出所選股票(全部區間)

			var stockData = _context.Stock
					.Where(x => x.SnCode == sncode)
					.OrderBy(x => x.StDate)
					.ToList();



			////測試區
			//foreach (var key in stockData) {

			//    Console.WriteLine(key.SnCode);

			//}
			//return Ok("Some result");


			var features = new List<float[]>();
			foreach (var item in selectedParams) {
				if (item.Value) {
					float[] featureColumn;
					switch (item.Key) {
						case "SteOpen":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteOpen ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteClose":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteClose ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteMax":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteMax ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteMin":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteMin ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteSpreadRatio":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteSpreadRatio ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteTradeMoney":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteTradeMoney ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteTradeQuantity":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteTradeQuantity ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteTransActions":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteTransActions ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteDividendYear":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteDividendYear ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbYield":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbYield ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbPbratio":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbPbratio ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbEps":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbEps ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbBussinessIncome":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbBussinessIncome ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMovingAverage5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMovingAverage5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMovingAverage30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMovingAverage30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiRsv5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiRsv5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiRsv30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiRsv30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiK5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiK5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiK30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiK30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiD5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiD5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiD30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiD30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiLongEma":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiLongEma ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiShortEma":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiShortEma ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiDif":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiDif ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMacd":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMacd ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiOsc":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiOsc ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiPe":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiPe ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMa":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMa ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						default:
							break;
					}
				}
			}

			////測試區
			//foreach (var feature in features)
			//{
			//	foreach (var value in feature)
			//	{
			//		Console.Write(value + " ");
			//	}
			//	Console.WriteLine();
			//}
			//return Ok("Some result");


			// 將數據轉換為 NumPy格式
			int rowcount = stockData.Count;//資料庫幾筆資料 28
			int colcount = features.Count;//客人選中幾個參數

			//測試區
			//Console.WriteLine(rowcount);
			//Console.WriteLine(colcount);
			//return Ok("Some result");

			// 創建 X 的 NumPy 數組，行數為股票數據的行數，列數為客戶選擇的特徵數量
			var x = np.zeros(new Shape(rowcount, colcount), dtype: np.float32);

			// 將客戶選擇的特徵添加到 X 的 NumPy 數組中
			for (int i = 0; i < colcount; i++) {
				var feature = features[i];
				for (int j = 0; j < rowcount; j++) {
					x[j, i] = feature[j]; // 將特徵數據填入 X 的 NumPy 數組中
				}
			}

			// 測試Xnumpy的樣子
			//string xString1 = x.ToString();
			//string filePathx1 = "x_array1.txt";
			//System.IO.File.WriteAllText(filePathx1, xString1);
			//Console.WriteLine("X NumPy array written to file: " + filePathx1);



			// Close 列作目標值 Y
			var yList = new List<float>();
			foreach (var data in stockData) {
				if (data.SteClose.HasValue) {
					yList.Add(Convert.ToSingle(data.SteClose));
				} else {
					continue;
				}
			}
			var y = np.array(yList.ToArray());

			//測試y
			//string yString1 = y.ToString();
			//string filePathy1 = "y_array1.txt";
			//System.IO.File.WriteAllText(filePathy1, yString1);
			//Console.WriteLine("y NumPy array written to file: " + filePathy1);

			// 模型構造
			Shape theShape = new Shape(rowcount, colcount, 1);
			x = np.reshape(x, theShape);
			var model = keras.Sequential();
			model.add(keras.layers.LSTM(64, keras.activations.Relu));
			model.add(keras.layers.Dense(64));
			model.add(keras.layers.Dense(1));
			model.compile(optimizer: keras.optimizers.Adam(), loss: keras.losses.MeanSquaredError());
			////測試Xnumpy的樣子
			//string xString1 = x.ToString();
			//string filePathx1 = "x_array1.txt";
			//System.IO.File.WriteAllText(filePathx1, xString1);
			//Console.WriteLine("X NumPy array written to file: " + filePathx1);

			//string xString2 = x.ToString();
			//string filePathx2 = "x_array2.txt";
			//System.IO.File.WriteAllText(filePathx2, xString2);
			//Console.WriteLine("X NumPy array written to file: " + filePathx2);

			//string yString2 = y.ToString();
			//string filePathy2 = "y_array2.txt";
			//System.IO.File.WriteAllText(filePathy2, yString2);
			//Console.WriteLine("y NumPy array written to file: " + filePathy2);
			//return Ok("Some result");

			//模型
			var history = model.fit(x, y, epochs: iterationtime, verbose: 0);
			// 獲取訓練過程中的 loss 值列表
			var lossList = history.history["loss"];

			// 獲取最後一個 epoch 的 loss 值
			var lastLoss = lossList.LastOrDefault();
			string lastLosstostring = lastLoss.ToString();
			// 打印最後一個 epoch 的 loss 值



			int stockDatafromtime = predictdatacount(sncode, predictday);

			Console.WriteLine(stockDatafromtime);

			//提取最後 predictday 天的特徵 X 的數據進行預測

			var lastData = new List<float[]>();
			for (int i = stockData.Count - stockDatafromtime; i < stockData.Count; i++) {
				float[] lastDataRow = new float[features.Count];
				for (int j = 0; j < features.Count; j++) {
					lastDataRow[j] = features[j][i];
				}
				lastData.Add(lastDataRow);
			}
			var reshapedData = np.zeros(new Shape(stockDatafromtime, features.Count, 1), dtype: np.float32);
			for (int i = 0; i < stockDatafromtime; i++) {
				for (int j = 0; j < features.Count; j++) {
					reshapedData[i, j, 0] = lastData[i][j];
				}
			}


			var prediction = model.predict(reshapedData);


			// 輸出預測結果
			predictionResult = prediction[0].numpy()[0, 0];
			predictionResulttoString = predictionResult.ToString();

			return Content($"{predictionResulttoString},{lastLosstostring}");

		}

		private int predictdatacount( string sncode, int predictday ) {
			DateTime currentday = DateTime.Parse("2024-03-08");
			DateTime previousdateTime = currentday.AddDays(-predictday);
			DateOnly previousdateOnly = DateOnly.Parse(previousdateTime.ToString("yyyy-MM-dd"));

			//測試
			Console.WriteLine("previousdateTime:" + previousdateTime);
			Console.WriteLine("previousdateOnly:" + previousdateOnly);

			var stockDatafromtime = _context.Stock.
				Where(x => x.StDate > previousdateOnly)
				.Where(x => x.SnCode == sncode)
				.OrderBy(x => x.StDate)
				.ToList();
			//foreach (var w in stockDatafromtime)
			//{


			//	Console.WriteLine(w.StDate.ToString());
			//	Console.WriteLine(w.SnName.ToString());
			//}
			//Console.WriteLine(stockDatafromtime.Count);
			predictday = stockDatafromtime.Count;
			return predictday;
		}

		[HttpPost]
		public IActionResult FNNpredict( string sncode, int predictday, Dictionary<string, bool> selectedParams, int iterationtime ) {
			//測試區
			// 在控制台輸出收到的資料以進行檢查
			//Console.WriteLine("Received data:");
			//Console.WriteLine("SN Code: " + sncode);	
			//Console.WriteLine("Predict Day: " + predictday);
			//Console.WriteLine("Selected Params:");
			//foreach (var key in selectedParams.Keys)
			//{
			//    Console.WriteLine(key);
			//}
			//return Ok("Some result");

			float predictionResult;
			string predictionResulttoString;
			int selsectedcount = selectedParams.Count;

			//撈出所選股票

			var stockData = _context.Stock
					.Where(x => x.SnCode == sncode)
					.OrderBy(x => x.StDate)
					.ToList();



			////測試區
			//foreach (var key in stockData) {

			//    Console.WriteLine(key.SnCode);

			//}
			//return Ok("Some result");


			var features = new List<float[]>();
			foreach (var item in selectedParams) {
				if (item.Value) {
					float[] featureColumn;
					switch (item.Key) {
						case "SteOpen":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteOpen ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteClose":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteClose ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteMax":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteMax ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteMin":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteMin ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteSpreadRatio":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteSpreadRatio ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteTradeMoney":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteTradeMoney / 10000 ?? 0)).ToArray();
							features.Add(featureColumn);

							break;
						case "SteTradeQuantity":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteTradeQuantity ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteTransActions":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteTransActions ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SteDividendYear":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SteDividendYear ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbYield":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbYield ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbPbratio":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbPbratio ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbEps":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbEps ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SbBussinessIncome":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SbBussinessIncome ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMovingAverage5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMovingAverage5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMovingAverage30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMovingAverage30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiRsv5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiRsv5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiRsv30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiRsv30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiK5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiK5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiK30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiK30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiD5":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiD5 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiD30":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiD30 ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiLongEma":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiLongEma ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiShortEma":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiShortEma ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiDif":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiDif ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMacd":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMacd ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiOsc":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiOsc ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiPe":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiPe ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						case "SiMa":
							featureColumn = stockData.Select(data => Convert.ToSingle(data.SiMa ?? 0)).ToArray();
							features.Add(featureColumn);
							break;
						default:
							break;
					}
				}
			}

			//測試區
			//foreach (var feature in features)
			//{
			//    foreach (var value in feature)
			//    {
			//        Console.Write(value + " ");
			//    }
			//    Console.WriteLine();
			//}
			//return Ok("Some result");
			foreach (var item in features) {
				System.Diagnostics.Debug.WriteLine("swith" + item);

			}


			// 將數據轉換為 NumPy格式
			int rowcount = stockData.Count;//資料庫幾筆資料 8
			int colcount = features.Count;//客人選中幾個參數

			//測試區
			//Console.WriteLine(rowcount);
			//Console.WriteLine(colcount);

			// 創建 X 的 NumPy 數組，行數為股票數據的行數，列數為客戶選擇的特徵數量
			var x = np.zeros(new Shape(rowcount, colcount), dtype: np.float32);

			// 將客戶選擇的特徵添加到 X 的 NumPy 數組中
			for (int i = 0; i < colcount; i++) {
				var feature = features[i];
				for (int j = 0; j < rowcount; j++) {
					x[j, i] = feature[j]; // 將特徵數據填入 X 的 NumPy 數組中
				}
			}

			// 測試Xnumpy的樣子
			//string xString = x.ToString();
			//string filePath = "x_array1.txt";
			//System.IO.File.WriteAllText(filePath, xString);
			//Console.WriteLine("X NumPy array written to file: " + filePath);



			// Close 列作目標值 Y
			var yList = new List<float>();
			foreach (var data in stockData) {
				if (data.SteClose.HasValue) {
					yList.Add(Convert.ToSingle(data.SteClose));
				} else {
					continue;
				}
			}
			var y = np.array(yList.ToArray());

			//測試y
			//string yString = y.ToString();
			//string filePath = "y_array.txt";
			//System.IO.File.WriteAllText(filePath, yString);
			//Console.WriteLine("y NumPy array written to file: " + filePath);
			//return Ok("Some result");
			// 模型構造
			//Feedforward Neural Network
			var model = keras.Sequential();
			model.add(keras.layers.Dense(64, activation: "relu", input_shape: new Shape(colcount)));
			model.add(keras.layers.Dense(64, activation: "relu", input_shape: new Shape(colcount)));
			model.add(keras.layers.Dense(1));
			//編譯模型
			model.compile(optimizer: keras.optimizers.Adam(), loss: keras.losses.MeanSquaredError());
			//訓練模型
			var history = model.fit(x, y, epochs: iterationtime, verbose: 0);
			// 獲取訓練過程中的 loss 值列表
			var lossList = history.history["loss"];

			// 獲取最後一個 epoch 的 loss 值
			var lastLoss = lossList.LastOrDefault();
			string lastLosstostring = lastLoss.ToString();
			// 打印最後一個 epoch 的 loss 值


			int stockDatafromtime = predictdatacount(sncode, predictday);

			Console.WriteLine(stockDatafromtime);

			// 提取最後 predictday 天的特徵 X 的數據進行預測
			var lastData = new List<float[]>();
			for (int i = stockData.Count - stockDatafromtime; i < stockData.Count; i++) {
				float[] lastDataRow = new float[features.Count];
				for (int j = 0; j < features.Count; j++) {
					lastDataRow[j] = features[j][i];
				}
				lastData.Add(lastDataRow);
			}


			// 創建預測數據的 numpy array
			var reshapedData = np.zeros(new Shape(1, stockDatafromtime, features.Count), dtype: np.float32);
			for (int i = 0; i < stockDatafromtime; i++) {
				for (int j = 0; j < features.Count; j++) {
					reshapedData[0, i, j] = lastData[i][j];
				}
			}

			// 進行預測
			var prediction = model.predict(reshapedData);
			// 輸出預測結果
			predictionResult = prediction[0].numpy()[0, 0];
			predictionResulttoString = predictionResult.ToString();

			return Content($"{predictionResulttoString},{lastLosstostring}");

		}


		public IActionResult predictindex() {
			return View();
		}
		public IActionResult predictphoto( string predicteddata, string sncode, string predictedloss ) {
			// 檢索資料庫中的資料筆數
			int dataCount = _context.Stock.Where(x => x.SnCode == sncode).Count();

			// 接收 predictedData 的值
			//System.Diagnostics.Debug.WriteLine("lookhere"+predicteddata);
			double predictedData = Convert.ToDouble(predicteddata);
			//Console.WriteLine(predictedData);

			// 將資料傳遞到視圖
			ViewBag.DataCount = dataCount;
			ViewBag.PredictedData = predictedData;
			ViewBag.PredictedLoss = predictedloss;
			// 檢索資料庫中的股票資料
			var stockData = _context.Stock.Where(x => x.SnCode == sncode).ToList();
			// 將股票資料傳遞到視圖
			ViewBag.ChartData = stockData;

			return View();
		}

		//public IActionResult checkstock( string sncode ) {
		//	var stockData = _context.Stock
		//			.Where(x => x.SnCode == sncode)
		//			.OrderBy(x => x.StDate)
		//			.ToList();
		//	string stockname;
		//	bool stockexist;
		//	if (stockData == null || stockData.Count == 0) {
		//		stockname = "查無這支股票";
		//		stockexist = false;
		//	} else {
		//		stockname = stockData[0].SnName;
		//		stockexist = true;

		//	}
		//	var result = new { stockname = stockname, stockexist = stockexist };
		//	return Json(result);
		//}

		[HttpPost]
		public IActionResult Predictsavedata( string PStock, string PVariable, decimal PLabel, byte PPrefer, string PBuildTime, string PfinishTime, string PAccount, string Pparameter ) {
			var query = new StocksContext();
			DateTime buildTime = DateTime.Parse(PBuildTime);
			DateTime finishTime = DateTime.Parse(PfinishTime);
			//System.Diagnostics.Debug.WriteLine($"PLabel: {PLabel}");
			System.Diagnostics.Debug.WriteLine($"Pparameter111111111111: {Pparameter}");
			var parametertodb = $"{{\"MSE\":{Pparameter}}}";
			var newdata = new DbModel {
				Pstock = PStock,
				Pvariable = PVariable,
				Plabel = PLabel,
				Pprefer = PPrefer,
				PbulidTime = buildTime,
				PfinishTime = finishTime,
				Paccount = PAccount,
				Dummyblock = parametertodb
			};
			//System.Diagnostics.Debug.WriteLine($"PbulidTime: {buildTime}");
			query.DbModels.Add(newdata);
			query.SaveChanges();

			var user = _context.DbMembers.FirstOrDefault(o => o.MaccoMnt == PAccount);
			if (user != null) {
				// 更新會員相關的屬性
				user.Mprefer = PPrefer; // 這裡請替換為您想要更新的屬性和值
				_context.DbMembers.Update(user);
				_context.SaveChanges();
			}
			return View();
		}

		[HttpGet]
		public IActionResult countpredicttime( string accountname ) {
			var query = _context.DbModels.Count(o => o.Paccount == accountname);
			bool result;
			if (query == 5) {
				result = true;
			} else {
				result = false;
			}
			var Finalresult = new { resulttoformer = result };
			return Json(Finalresult);
		}
	}
}
