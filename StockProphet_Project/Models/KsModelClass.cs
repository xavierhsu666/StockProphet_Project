using System.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Trainers.LightGbm;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.ML.Transforms.HashingEstimator;
using Microsoft.ML.Transforms.TimeSeries;
using System.Data.SqlClient;
using static Microsoft.ML.ForecastingCatalog;
using System.Xml.Linq;

namespace StockProphet_Project.Models {


	public class KsModelClass {
		public class Prediction {
			[ColumnName("Score")]
			public float STe_Close { get; set; }
		}
		public Prediction MLModeling( List<Stock> input_untrans ) {
			MLContext mlContext = new MLContext();
			var input = TransferToInput(input_untrans);
			// 1. Import or create training data
			IDataView trainingData = mlContext.Data.LoadFromEnumerable(input);
			// 2. Specify data preparation and model training pipeline
			//var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "S_PK","ST_Date",
			//    "ST_Quarter","ST_Year","SN_Code","SN_Name","STe_Open","STe_Close","STe_Max","STe_Min","STe_SpreadRatio",
			//    "STe_TradeMoney","STe_TradeQuantity","SB_EPS","SB_BussinessIncome","SB_NonBussinessIncome",
			//    "SB_NonBussinessIncomeRatio","SI_MovingAverage_5","SI_MovingAverage_30","SI_RSV_5","SI_RSV_30",
			//    "SI_K_5","SI_K_30","SI_D_5","SI_D_30","SI_EMA","SI_ShortEMA","SI_Dif","SI_MACD", "SI_OSC", "SI_PE"  })
			//    .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "STe_Close", maximumNumberOfIterations: 100));
			// 線性回歸
			var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "STe_Open","STe_Close","STe_Max",
				"STe_Min","STe_SpreadRatio"})
				.Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "STe_Close", maximumNumberOfIterations: 100));

			// 決策樹(二元預測)
			//        var pipeline = mlContext.Transforms.Concatenate("Features", new[] { "STe_Open", "STe_Close", "STe_Max",
			//"STe_Min", "STe_SpreadRatio" })
			//            .Append(mlContext.BinaryClassification.Trainers.LightGbm(labelColumnName: "STe_Close"));

			// 3. Train model
			var model = pipeline.Fit(trainingData);

			// 4. Make a prediction
			var x = input[input.Count - 1];
			var close = mlContext.Model.CreatePredictionEngine<StockDBA, Prediction>(model).Predict(x);
			// 5. 評估誤差
			return close;
			// Predicted price for size: 2500 sq ft= $261.98k
		}
		public List<StockDBA> TransferToInput( List<Stock> input ) {
			List<StockDBA> output = new List<StockDBA>();

			foreach (var item in input) {
				output.Add(new StockDBA() {
					SB_BussinessIncome = Convert.ToInt64(item.SbBussinessIncome),
					SB_EPS = Convert.ToSingle(item.SbEps),
					SB_PBRatio = Convert.ToSingle(item.SbPbratio),
					SB_Yield = Convert.ToSingle(item.SbYield),
					SI_Dif = Convert.ToSingle(item.SiDif),
					SI_D_30 = Convert.ToSingle(item.SiD30),
					SI_D_5 = Convert.ToSingle(item.SiD5),
					SI_K_30 = Convert.ToSingle(item.SiK30),
					SI_K_5 = Convert.ToSingle(item.SiK5),
					SI_LongEMA = Convert.ToSingle(item.SiLongEma),
					SI_MA = Convert.ToSingle(item.SiMa),
					SI_MACD = Convert.ToSingle(item.SiMacd),
					SI_MovingAverage_30 = Convert.ToSingle(item.SiMovingAverage30),
					SI_MovingAverage_5 = Convert.ToSingle(item.SiMovingAverage5),
					SI_OSC = Convert.ToSingle(item.SiOsc),
					SI_PE = Convert.ToSingle(item.SiPe),
					SI_RSV_30 = Convert.ToSingle(item.SiRsv30),
					SI_RSV_5 = Convert.ToSingle(item.SiRsv5),
					SI_ShortEMA = Convert.ToSingle(item.SiShortEma),
					SN_Code = item.SnCode,
					SN_Name = item.SnName,
					STe_Close = Convert.ToSingle(item.SteClose),
					STe_Dividend_Year = Convert.ToSingle(item.SteDividendYear),
					STe_Max = Convert.ToSingle(item.SteMax),
					STe_Min = Convert.ToSingle(item.SteMin),
					STe_Open = Convert.ToSingle(item.SteOpen),
					STe_SpreadRatio = Convert.ToSingle(item.SteSpreadRatio),
					STe_TradeMoney = Convert.ToInt64(item.SteTradeMoney),
					STe_TradeQuantity = Convert.ToInt32(item.SteTradeQuantity),
					STe_TransActions = Convert.ToInt32(item.SteTransActions),
					ST_Date = DateTime.Parse(item.StDate.ToString()),
					ST_Quarter = item.StQuarter,
					ST_Year = item.StYear,
					ST_Year_Quarter = item.StYearQuarter,
					S_PK = item.SPk

				});
			}
			return output;
		}

	}
	public class TimeSerialModel {

		public class ModelOutput {
			public float[] ForecastedRentals { get; set; }

			public float[] LowerBoundRentals { get; set; }

			public float[] UpperBoundRentals { get; set; }
		}
		public class ModelPara {
			public DateTime focastDate { get; set; }
			public int windowSize { get; set; }
			public int seriesLength { get; set; }
			public int trainSize { get; set; }
			public Single confidenceLevel { get; set; }
			public double Output_M_MAE { get; set; }
			public double Output_M_RMSE { get; set; }
			public float Output_F_actualRentals { get; set; }
			public float Output_F_lowerEstimate { get; set; }
			public float Output_F_estimate { get; set; }
			public float Output_F_upperEstimate { get; set; }

		}

		public ModelPara KsMLModeling( List<Stock> input_untrans, ModelPara mp ) {

			// 建立需要使用的物件
			MLContext mlContext = new MLContext();
			KsModelClass mc = new KsModelClass();

			// 將資料庫回傳格式轉成input格式
			var input = mc.TransferToInput(input_untrans);

			// 將input格式轉成IDataView
			IDataView dataView = mlContext.Data.LoadFromEnumerable(input);

			// 檔案切割
			// IDataView firstYearData = mlContext.Data.FilterRowsByColumn(dataView, "ST_Date", upperBound: 1);
			// IDataView secondYearData = mlContext.Data.FilterRowsByColumn(dataView, "ST_Date", lowerBound: 1);

			// 設定模型參數
			var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
			outputColumnName: "ForecastedRentals",
			inputColumnName: "STe_Close",
			windowSize: mp.windowSize,
			seriesLength: mp.seriesLength,
			trainSize: mp.trainSize,
			horizon: 7,
			confidenceLevel: mp.confidenceLevel,
			confidenceLowerBoundColumn: "LowerBoundRentals",
			confidenceUpperBoundColumn: "UpperBoundRentals");

			// 模型建立
			SsaForecastingTransformer forecaster = forecastingPipeline.Fit(dataView);
			// 模型評估
			var modelEV = Evaluate(dataView, forecaster, mlContext);
			// 轉成固態模型
			var forecastEngine = forecaster.CreateTimeSeriesEngine<StockDBA, ModelOutput>(mlContext);
			// 調整預測用input值
			var q = from o in input
					where o.ST_Date == mp.focastDate
			select o;

			IDataView dataView1 = mlContext.Data.LoadFromEnumerable(q);
			// 進行預測
			var forcastEV = Forecast(dataView1, 7, forecastEngine, mlContext);
			return new ModelPara() {
				Output_M_MAE = modelEV[0],
				Output_M_RMSE = modelEV[1],
				Output_F_actualRentals = forcastEV[0],
				Output_F_lowerEstimate = forcastEV[1],
				Output_F_estimate = forcastEV[2],
				Output_F_upperEstimate = forcastEV[3],
			};

		}
		public double[] Evaluate( IDataView testData, ITransformer model, MLContext mlContext ) {
			IDataView predictions = model.Transform(testData);
			IEnumerable<float> actual =
			mlContext.Data.CreateEnumerable<StockDBA>(testData, true)
				.Select(observed => observed.STe_Close);
			IEnumerable<float> forecast =
			mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true)
				.Select(prediction => prediction.ForecastedRentals[0]);
			var metrics = actual.Zip(forecast, ( actualValue, forecastValue ) => actualValue - forecastValue);
			var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
			var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error
			Console.WriteLine("Evaluation Metrics");
			Console.WriteLine("---------------------");
			Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
			Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");
			return new double[] { (double)MAE, RMSE };
		}
		public float[] Forecast( IDataView testData, int horizon, TimeSeriesPredictionEngine<StockDBA, ModelOutput> forecaster, MLContext mlContext ) {
			ModelOutput forecast = forecaster.Predict();
			float actualRentals = 0;
			float lowerEstimate = 0;
			float upperEstimate = 0;
			float estimate = 0;
			IEnumerable<string> forecastOutput =
			mlContext.Data.CreateEnumerable<StockDBA>(testData, reuseRowObject: false)
				.Take(horizon)
				.Select(( StockDBA rental, int index ) => {
					string rentalDate = rental.ST_Date.ToShortDateString();
					actualRentals = rental.STe_Close;
					lowerEstimate = Math.Max(0, forecast.LowerBoundRentals[index]);
					estimate = forecast.ForecastedRentals[index];
					upperEstimate = forecast.UpperBoundRentals[index];
					return $"Date: {rentalDate}\n" +
					$"Actual Rentals: {actualRentals}\n" +
					$"Lower Estimate: {lowerEstimate}\n" +
					$"Forecast: {estimate}\n" +
					$"Upper Estimate: {upperEstimate}\n";
				});
			Console.WriteLine("Rental Forecast");
			Console.WriteLine("---------------------");
			foreach (var prediction in forecastOutput) {
				Console.WriteLine(prediction);
			}
			return new float[] { actualRentals, lowerEstimate, estimate, upperEstimate };
		}
	}
}
