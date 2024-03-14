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
		public string InputLatestDate = DateTime.Parse("2024-1-22").ToString("yyyy-MM-dd");
		public int InputMinCount = 30;
		// <參數區>改過需要調整的地方

		// <view頁面區>
		public IActionResult TestBuild() {// 获取表单中的所有输入值
			return View();
		}
		public IActionResult Build( [FromForm] IFormCollection form ) {// 获取表单中的所有输入值


			// 先看一下DB內的資料是否最新

			var q = from o in _context.Stock
					where o.StDate < DateOnly.Parse(InputLatestDate) &&
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
					string[] inputVar = new string[]{"STe_Open","STe_Close","STe_Max","STe_Min","STe_SpreadRatio"};
					KsModelClass.ModelInput mi = new KsModelClass.ModelInput() { 
						inputPara = inputVar ,
						maximumNumberOfIterations=100
					};	
					
					var mo = RegessionBuild(q, mi);
					//mo.output_F_Forcast,mo.output_M_RMSE,mo.output_M_MSE
					ViewBag.result = new double[] { mo.output_F_Forcast, mo.output_M_RMSE, mo.output_M_MSE };
					log += "Regression Builded. ";

					break;
				case "TimeSerial":
					log = "TimeSerial Building...";
					ViewBag.result = TimeSerialBuild().Output_F_estimate;
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
		public TimeSerialModel.ModelPara TimeSerialBuild() {// 获取表单中的所有输入值
			TimeSerialModel t = new TimeSerialModel();
			TimeSerialModel.ModelPara mp = new TimeSerialModel.ModelPara() {
				windowSize = 7,
				seriesLength = 30,
				trainSize = 365,
				confidenceLevel = 0.95f,
				focastDate = DateTime.Parse("2024/3/8")

			};

			var q = from o in _context.Stock.ToList()
					where o.SnCode == "2330"
					select o;
			var ModelReturn = t.KsMLModeling(q.ToList(), mp);
			return ModelReturn;
		}

		public KsModelClass.ModelOutput RegessionBuild( IQueryable<Stock> q , KsModelClass.ModelInput mi) {// 获取表单中的所有输入值
			KsModelClass mc = new KsModelClass();
			var a = mc.MLModeling(q.ToList(),mi);
			return a;
		}
		// <功能開發測試區>

		// <WEB API 區>
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
		// <WEB API 區>
		// <類別區>
		public class APIClass {
			public string stockCode { get; set; }
		}
		// <類別區>
		// ------------------------------------------------------------------------------<KAZUO>------------------------------------------------------------------------------
		
		
		
		
		
		// GET: Stockinfoes
		public async Task<IActionResult> Index() {
			return View(await _context.Stock.ToListAsync());
		}
		[HttpPost]
		public IActionResult LSTMpredict( string sncode, int predictday, Dictionary<string, bool> selectedParams ) {
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

			//判斷客戶所選區間資料是否足夠
			if (stockData.Count < predictday) {
				//測試區
				//Console.WriteLine("no enough data");
				//Console.WriteLine("stockData");
				//Console.WriteLine(stockData.Count);
				//Console.WriteLine("predictday");
				//Console.WriteLine(predictday);
				return BadRequest("現有資料不足，請重新選擇較小的區間");
			} else {
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
				model.add(keras.layers.LSTM(50, keras.activations.Relu));
				model.add(keras.layers.Dense(1));
				model.compile(optimizer: keras.optimizers.Adam(), loss: keras.losses.MeanSquaredError());
				// 測試Xnumpy的樣子
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
				model.fit(x, y, epochs: 10, verbose: 0);




				//提取最後 predictday 天的特徵 X 的數據進行預測

				var lastData = new List<float[]>();
				for (int i = stockData.Count - predictday; i < stockData.Count; i++) {
					float[] lastDataRow = new float[features.Count];
					for (int j = 0; j < features.Count; j++) {
						lastDataRow[j] = features[j][i];
					}
					lastData.Add(lastDataRow);
				}
				var reshapedData = np.zeros(new Shape(predictday, features.Count, 1), dtype: np.float32);
				for (int i = 0; i < predictday; i++) {
					for (int j = 0; j < features.Count; j++) {
						reshapedData[i, j, 0] = lastData[i][j];
					}
				}


				var prediction = model.predict(reshapedData);


				// 輸出預測結果
				predictionResult = prediction[0].numpy()[0, 0];

			}
			return Content(predictionResult.ToString());

		}

		[HttpPost]
		public IActionResult FNNpredict( string sncode, int predictday, Dictionary<string, bool> selectedParams ) {
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

			//判斷客戶所選區間資料是否足夠
			if (stockData.Count < predictday) {
				//測試區
				//Console.WriteLine("no enough data");
				//Console.WriteLine("stockData");
				//Console.WriteLine(stockData.Count);
				//Console.WriteLine("predictday");
				//Console.WriteLine(predictday);
				return BadRequest("現有資料不足，請重新選擇較小的區間");
			} else {
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
				//string filePath = "x_array.txt";
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
				model.add(keras.layers.Dense(1));
				//編譯模型
				model.compile(optimizer: keras.optimizers.Adam(), loss: keras.losses.MeanSquaredError());
				//訓練模型
				model.fit(x, y, epochs: 50, verbose: 0);




				// 提取最後 predictday 天的特徵 X 的數據進行預測
				var lastData = new List<float[]>();
				for (int i = stockData.Count - predictday; i < stockData.Count; i++) {
					float[] lastDataRow = new float[features.Count];
					for (int j = 0; j < features.Count; j++) {
						lastDataRow[j] = features[j][i];
					}
					lastData.Add(lastDataRow);
				}

				// 創建預測數據的 numpy array
				var reshapedData = np.zeros(new Shape(1, predictday, features.Count), dtype: np.float32);
				for (int i = 0; i < predictday; i++) {
					for (int j = 0; j < features.Count; j++) {
						reshapedData[0, i, j] = lastData[i][j];
					}
				}

				// 進行預測
				var prediction = model.predict(reshapedData);
				// 輸出預測結果
				predictionResult = prediction[0].numpy()[0, 0];

			}
			return Content(predictionResult.ToString());

		}


		public IActionResult predictindex() {
			return View();
		}
		public IActionResult predictphoto( string predicteddata, string sncode ) {
			// 檢索資料庫中的資料筆數
			int dataCount = _context.Stock.Where(x => x.SnCode == sncode).Count();

			// 接收 predictedData 的值
			double predictedData = Convert.ToDouble(predicteddata);


			// 將資料傳遞到視圖
			ViewBag.DataCount = dataCount;
			ViewBag.PredictedData = predictedData;

			// 檢索資料庫中的股票資料
			var stockData = _context.Stock.Where(x => x.SnCode == sncode).ToList();
			// 將股票資料傳遞到視圖
			ViewBag.ChartData = stockData;

			return View();
		}

		[HttpGet]
		public IActionResult predictdatacount( string sncode, int predictday ) {
			//撈出所選股票
			var stockData = _context.Stock
				.Where(x => x.SnCode == sncode)
				.OrderBy(x => x.StDate)
				.ToList();
			bool result;


			if (stockData.Count < predictday) {
				result = false;
			} else {
				result = true;
			}

			return Json(new { success = result });
		}
		public IActionResult checkstock( string sncode ) {
			var stockData = _context.Stock
					.Where(x => x.SnCode == sncode)
					.OrderBy(x => x.StDate)
					.ToList();
			string stockname;
			bool stockexist;
			if (stockData == null || stockData.Count == 0) {
				stockname = "查無這支股票";
				stockexist = false;
			} else {
				stockname = stockData[0].SnName;
				stockexist = true;

			}
			var result = new { stockname = stockname, stockexist = stockexist };
			return Json(result);
		}

		[HttpPost]
		public IActionResult Predictsavedata( string PStock, string PVariable, decimal PLabel, byte PPrefer, string PBuildTime ) {
			var query = new StocksContext();
			DateTime buildTime = DateTime.Parse(PBuildTime);
			var newdata = new DbModel {
				Pstock = PStock,
				Pvariable = PVariable,
				Plabel = PLabel,
				Pprefer = PPrefer,
				PbulidTime = buildTime
			};
			System.Diagnostics.Debug.WriteLine($"PbulidTime: {buildTime}");
			query.DbModels.Add(newdata);
			query.SaveChanges();

			return View();
		}
	}
}
