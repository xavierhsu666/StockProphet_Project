using HtmlAgilityPack;
using Humanizer;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Tensorflow.Common.Types;

namespace StockProphet_Project.Models {
	public class WebAPI_Class {
		public string stockCode { get; set; }
		public string stockName { get; set; }
		public DateTime date { get; set; }
		public class Table1OBJ {
			//日期 成交股數    成交金額 開盤價 最高價 最低價 收盤價 漲跌價差    成交筆數
			public DateTime date { get; set; }
			public Single transQ { get; set; }
			public Single transP { get; set; }
			public string stockCode { get; set; }
			public Single OpenPrice { get; set; }
			public Single MaxPrice { get; set; }
			public Single MinPrice { get; set; }
			public Single ClosePrice { get; set; }
			public Single PriceGap { get; set; }
			public Single DealCount { get; set; }
		}
		public class Table2OBJ {
			//日期	殖利率(%)	股利年度	本益比	股價淨值比	財報年/季
			public DateTime date { get; set; }
			public Single SB_Yield { get; set; }
			public Single STe_Dividend_Year { get; set; }
			public Single SI_PE { get; set; }
			public Single SB_PBRatio { get; set; }
			public string ST_Year_Quarter { get; set; }

		}
		public class Table3OBJ {
			public string stockCode { get; set; }
			public string stockName { get; set; }
			public Single? Yield { get; set; }
		}

		public class TableOBJ {
			public string stockCode { get; set; }
			public string stockName { get; set; }
			public Single? Yield { get; set; }// 月營收
			public DateTime date { get; set; }
			public Single SB_Yield { get; set; }
			public Single STe_Dividend_Year { get; set; }
			public Single SI_PE { get; set; }
			public Single SB_PBRatio { get; set; }
			public string ST_Year_Quarter { get; set; }
			public Single transQ { get; set; }
			public Single transP { get; set; }
			public Single OpenPrice { get; set; }
			public Single MaxPrice { get; set; }
			public Single MinPrice { get; set; }
			public Single ClosePrice { get; set; }
			public Single PriceGap { get; set; }
			public Single DealCount { get; set; }

		}

		[EnableCors("MyAllowSpecificOrigins")]
		[HttpGet]
		public async Task<List<Table3OBJ>> ajax_3() {
			List<Table3OBJ> table3OBJs = new List<Table3OBJ>();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			List<string> stockCode = new List<string>();
			List<string> stockName = new List<string>();
			List<Single> Yield = new List<Single>();
			using (HttpClient client = new HttpClient()) {
				try {
					CultureInfo culture = new CultureInfo("zh-TW");
					culture.DateTimeFormat.Calendar = new TaiwanCalendar();
					// 設定 Web API 的 URL
					//string apiUrl = "https://mops.twse.com.tw/nas/t21/sii/t21sc03_113_2_0.html";
					string apiUrl = "https://mops.twse.com.tw/nas/t21/sii/t21sc03_" + this.date.ToString("yyy_M", culture) + "_0.html";
					Console.WriteLine(this.date.ToString("yyy_M", culture));
					// 發送 GET 請求並等待回應
					HttpResponseMessage response = await client.GetAsync(apiUrl);

					// 確認請求成功
					if (response.IsSuccessStatusCode) {
						// 讀取回傳的資料
						byte[] bytes = await response.Content.ReadAsByteArrayAsync();
						// 將資料轉換為 UTF-8 編碼
						string responseBody = Encoding.GetEncoding("big5").GetString(bytes);
						// Load the HTML content into an HtmlDocument object
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(responseBody);

						// Select the inner table using XPath and display its td elements
						HtmlNode outerTable = doc.DocumentNode.SelectSingleNode("//table");
						int k = 0;
						if (outerTable != null) {

							HtmlNodeCollection innerTables = outerTable.SelectNodes(".//table");
							//foreach (HtmlNode innerTable in innerTables) {
							for (int j = 1; j < innerTables.Count; j++) {
								if (j % 2 != 0) {
									var innerTable = innerTables[j];

									if (innerTable != null) {

										HtmlNodeCollection tdNodes = innerTable.SelectNodes(".//td");
										if (tdNodes != null) {
											//Console.WriteLine("Inner table td elements" + tdNodes.Count + ":");
											int i = 1;

											foreach (HtmlNode tdNode in tdNodes) {
												if (i % 11 >= 1 && i % 11 <= 3 && i < tdNodes.Count - 8) {
													switch (i % 11) {
														case 1:
															stockCode.Add(tdNode.InnerText);
															break;
														case 2:
															stockName.Add(tdNode.InnerText);
															break;
														case 3:
															Yield.Add(Convert.ToSingle(tdNode.InnerText.Replace(",", "")));
															break;
													}
													//Console.WriteLine("(" + i + ")(" + i % 11 + ")" + tdNode.InnerText);
													k++;
												}
												i++;
											}


										}
									}
								}
							}


						}
						if (stockCode.Count == stockName.Count && stockName.Count == Yield.Count) {
							for (int i = 0; i < stockCode.Count; i++) {
								var tmpObj = new Table3OBJ();
								tmpObj.stockName = stockName[i];
								tmpObj.stockCode = stockCode[i];
								tmpObj.Yield = Yield[i];
								table3OBJs.Add(tmpObj);
							}
						}
						return table3OBJs;
						//Console.WriteLine(text);
						//foreach (var item in list) { //正则表达式获取每行所有的
						//	Console.WriteLine(item);
						//}
					} else {
						Console.WriteLine($"請求失敗，狀態碼：{response.StatusCode}");
						return new List<Table3OBJ> { };
					}
				} catch (HttpRequestException e) {
					Console.WriteLine($"發生 HTTP 錯誤： {e.Message}");
					return new List<Table3OBJ> { };
				}
			}

		}
		[HttpGet]
		public async Task<List<Table1OBJ>> ajax_1() {
			List<Table1OBJ> table1OBJs = new List<Table1OBJ>();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			List<DateTime> date = new List<DateTime>();
			List<Single> transQ = new List<Single>();
			List<Single> transP = new List<Single>();
			List<Single> OpenPrice = new List<Single>();
			List<Single> DealCount = new List<Single>();
			List<Single> PriceGap = new List<Single>();
			List<Single> ClosePrice = new List<Single>();
			List<Single> MinPrice = new List<Single>();
			List<Single> MaxPrice = new List<Single>();

			using (HttpClient client = new HttpClient()) {
				try {
					// 設定 Web API 的 URL
					string apiUrl = "https://www.twse.com.tw/exchangeReport/STOCK_DAY?response=html&date=" + this.date.ToString("yyyyMMdd") + "&stockNo=" + this.stockCode;

					// 發送 GET 請求並等待回應
					HttpResponseMessage response = await client.GetAsync(apiUrl);

					// 確認請求成功
					if (response.IsSuccessStatusCode) {
						// 讀取回傳的資料
						byte[] bytes = await response.Content.ReadAsByteArrayAsync();
						// 將資料轉換為 UTF-8 編碼
						string responseBody = Encoding.GetEncoding("big5").GetString(bytes);
						// Load the HTML content into an HtmlDocument object
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(responseBody);

						// Select the inner table using XPath and display its td elements
						HtmlNode outerTable = doc.DocumentNode.SelectSingleNode("//table");
						int k = 0;
						if (outerTable != null) {
							HtmlNodeCollection tdNodes = outerTable.SelectNodes(".//td");
							if (tdNodes != null) {
								Console.WriteLine("Inner table td elements" + tdNodes.Count + ":");
								int i = 1;
								foreach (HtmlNode tdNode in tdNodes) {
									switch (i % 9) {
										//日期 成交股數    成交金額 開盤價 最高價 最低價 收盤價 漲跌價差    成交筆數
										case 1:
											date.Add(DateTime.Parse(tdNode.InnerText));
											break;
										case 2:
											transQ.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 3:
											transP.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 4:
											OpenPrice.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 5:
											MaxPrice.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 6:
											MinPrice.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 7:
											ClosePrice.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 8:
											PriceGap.Add((tdNode.InnerText.IndexOf("X") >= 0) ? 0 : Convert.ToSingle(tdNode.InnerText));
											break;
										case 0:
											DealCount.Add(Convert.ToSingle(tdNode.InnerText));
											break;
									}

									k++;
									i++;
								}
							}
						}
						for (int i = 0; i < DealCount.Count; i++) {
							var tmp1OBJ = new Table1OBJ();
							tmp1OBJ.PriceGap = PriceGap[i];
							tmp1OBJ.date = date[i];
							tmp1OBJ.transQ = transQ[i];
							tmp1OBJ.transP = transP[i];
							tmp1OBJ.date = date[i];
							tmp1OBJ.OpenPrice = OpenPrice[i];
							tmp1OBJ.ClosePrice = ClosePrice[i];
							tmp1OBJ.MaxPrice = MaxPrice[i];
							tmp1OBJ.MinPrice = MinPrice[i];
							tmp1OBJ.DealCount = DealCount[i];
							tmp1OBJ.stockCode = this.stockCode;
							table1OBJs.Add(tmp1OBJ);
						}
						return table1OBJs;

						//Console.WriteLine(text);
						//foreach (var item in list) { //正则表达式获取每行所有的
						//	Console.WriteLine(item);
						//}
					} else {
						Console.WriteLine($"請求失敗，狀態碼：{response.StatusCode}");
						return new List<Table1OBJ> { };
					}
					Console.WriteLine("//////////6");

				} catch (HttpRequestException e) {
					Console.WriteLine($"發生 HTTP 錯誤： {e.Message}");
					return new List<Table1OBJ> { };
				}
			}

		}
		[HttpGet]
		public async Task<List<Table2OBJ>> ajax_2() {
			List<Table2OBJ> table2OBJs = new List<Table2OBJ>();
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			List<DateTime> date = new List<DateTime>();
			List<Single> SB_Yield = new List<Single>();
			List<Single> STe_Dividend_Year = new List<Single>();
			List<Single> SI_PE = new List<Single>();
			List<Single> SB_PBRatio = new List<Single>();
			List<string> ST_Year_Quarter = new List<string>();

			using (HttpClient client = new HttpClient()) {
				try {
					// 設定 Web API 的 URL
					string apiUrl = "https://www.twse.com.tw/rwd/zh/afterTrading/BWIBBU?response=html&date=" + this.date.ToString("yyyyMMdd") + "&stockNo=" + this.stockCode;

					// 發送 GET 請求並等待回應
					HttpResponseMessage response = await client.GetAsync(apiUrl);

					// 確認請求成功
					if (response.IsSuccessStatusCode) {
						// 讀取回傳的資料
						byte[] bytes = await response.Content.ReadAsByteArrayAsync();
						// 將資料轉換為 UTF-8 編碼
						//string responseBody = Encoding.GetEncoding("big5").GetString(bytes);
						string responseBody = Encoding.UTF8.GetString(bytes);
						// Load the HTML content into an HtmlDocument object
						HtmlDocument doc = new HtmlDocument();
						doc.LoadHtml(responseBody);

						// Select the inner table using XPath and display its td elements
						HtmlNode outerTable = doc.DocumentNode.SelectSingleNode("//table");
						int k = 0;
						if (outerTable != null) {
							HtmlNodeCollection tdNodes = outerTable.SelectNodes(".//td");
							if (tdNodes != null) {
								Console.WriteLine("Inner table td elements" + tdNodes.Count + ":");
								int i = 1;
								foreach (HtmlNode tdNode in tdNodes) {
									switch (i % 6) {
										//日期	殖利率(%)	股利年度	本益比	股價淨值比	財報年/季
										case 1:
											date.Add(DateTime.Parse(tdNode.InnerText));
											break;
										case 2:
											SB_Yield.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 3:
											STe_Dividend_Year.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 4:
											SI_PE.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 5:
											SB_PBRatio.Add(Convert.ToSingle(tdNode.InnerText));
											break;
										case 0:
											ST_Year_Quarter.Add(tdNode.InnerText);
											break;
									}
									//Console.WriteLine("("+i+")("+i%6+")"+tdNode.InnerText);

									k++;
									i++;
								}
							}
						}
						for (int i = 0; i < date.Count; i++) {
							var tmp2OBJ = new Table2OBJ();
							tmp2OBJ.ST_Year_Quarter = ST_Year_Quarter[i];
							tmp2OBJ.date = date[i];
							tmp2OBJ.SI_PE = SI_PE[i];
							tmp2OBJ.STe_Dividend_Year = STe_Dividend_Year[i];
							tmp2OBJ.SB_Yield = SB_Yield[i];
							tmp2OBJ.SB_PBRatio = SB_PBRatio[i];
							table2OBJs.Add(tmp2OBJ);
						}
						return table2OBJs;

						//Console.WriteLine(text);
						//foreach (var item in list) { //正则表达式获取每行所有的
						//	Console.WriteLine(item);
						//}
					} else {
						Console.WriteLine($"請求失敗，狀態碼：{response.StatusCode}");
						return new List<Table2OBJ> { };
					}
					Console.WriteLine("//////////6");

				} catch (HttpRequestException e) {
					Console.WriteLine($"發生 HTTP 錯誤： {e.Message}");
					return new List<Table2OBJ> { };
				}
			}
		}
		public bool CheckPara() {
			if (this.stockCode is null || this.stockName is null || this.date == DateTime.MinValue) {
				return false;
			} else {
				return true;
			}
		}
		public async Task<List<Stock>> mergeTable() {
			if (CheckPara()) {
				var t1 = await ajax_1();
				var t2 = await ajax_2();
				var t3 = await ajax_3();
				List<Stock> mergedList = new List<Stock>();
				for (int i = 0; i < Math.Min(t1.Count, t2.Count); i++) {
					// 抓t2
					var q = from o in t2
							where o.date == t1[i].date
							select o;
					// 抓t3
					var q3 = from o in t3
							 where o.stockCode == this.stockCode
							 select o;
					if (q.Count() == 1) {
						Stock tmpTable = new Stock();
						tmpTable.SnCode = this.stockCode;
						tmpTable.StDate = DateOnly.Parse(t1[i].date.ToString("yyyy-MM-dd"));
						tmpTable.SteClose = Convert.ToDecimal(t1[i].ClosePrice);
						tmpTable.SteMax = Convert.ToDecimal(t1[i].MaxPrice);
						tmpTable.SteMin = Convert.ToDecimal(t1[i].MinPrice);
						tmpTable.SteOpen = Convert.ToDecimal(t1[i].OpenPrice);
						tmpTable.SteTradeMoney = Convert.ToInt64(t1[i].transP);
						tmpTable.SteTradeQuantity = Convert.ToInt32(t1[i].transQ);
						tmpTable.SteTransActions = Convert.ToInt32(t1[i].DealCount);
						tmpTable.SiPe = t2[i].SI_PE;
						tmpTable.SbPbratio = t2[i].SB_PBRatio;
						tmpTable.SbYield = Convert.ToDecimal(t2[i].SB_Yield);
						tmpTable.SteDividendYear = Convert.ToByte(t2[i].STe_Dividend_Year);
						tmpTable.StYearQuarter = t2[i].ST_Year_Quarter;
						tmpTable.SnName = this.stockName;
						tmpTable.SbBussinessIncome = Convert.ToInt64(q3.FirstOrDefault(new Table3OBJ() { stockCode = this.stockCode, stockName = "查無資料", Yield = null }).Yield);
						mergedList.Add(tmpTable);
					}
				}
				//Console.WriteLine("Merged list:");
				//foreach (var item in mergedList) {
				//	Console.WriteLine("date : " + item.StDate + " " +
				//		"stockCode : " + item.SnCode + " " +
				//		"stockName : " + item.SnName + " " +
				//		"SB_PBRatio : " + item.SbPbratio + " " +
				//		"SI_PE : " + item.SiPe + " " +
				//		"SB_Yield : " + item.SbYield + " " +
				//		"STe_Dividend_Year : " + item.SteDividendYear + " " +
				//		"ST_Year_Quarter : " + item.StYearQuarter + " " +
				//		"ClosePrice : " + item.SteClose + " " +
				//		"OpenPrice : " + item.SteOpen + " " +
				//		"MaxPrice : " + item.SteMax + " " +
				//		"MinPrice : " + item.SteMin + " " +
				//		"transP : " + item.SteTradeMoney + " " +
				//		"transQ : " + item.SteTradeQuantity + " " +
				//		"yield : " + item.SbBussinessIncome + " "
				//		);
				//}
				return mergedList;
			} else {

				Console.WriteLine("Stock API呼叫失敗 : 參數有少~~");
				return new List<Stock> { };
			}


		}
		public void calIndex( List<Stock> stocks ) {
			// 計算五日移動平均
			var NewStocks_1 = CalculateMovingAverage(stocks, 5);

			// 計算三十日移動平均
			var NewStocks_2 = CalculateMovingAverage(stocks, 5);
			foreach (var item in NewStocks_2) {
				Console.WriteLine(
					"SteClose = " + item.SteClose +
					", aver5 = " + item.SiMovingAverage5 +
					", aver30 = " + item.SiMovingAverage30 +
					" ");
			}

		}
		private readonly StocksContext _context;
		public async void run() {
			var stocks =await mergeTable();
			var q = from o in _context.Stock.ToList() 
					where o.SnCode == this.stockCode
					orderby o.StDate descending
					select o ;

			calIndex(stocks);
		}
		private List<Stock> CalculateMovingAverage( List<Stock> stocks, int period ) {
			for (int i = period - 1; i < stocks.Count; i++) {
				double sum = 0;
				for (int j = i - (period - 1); j <= i; j++) {
					sum += (double)((stocks[j].SteClose) ?? 0);
				}
				double movingAverage = sum / period;
				if (period == 5) {
					stocks[i].SiMovingAverage5 = movingAverage;
				} else if (period == 30) {
					stocks[i].SiMovingAverage30 = movingAverage;
				}

			}
			return stocks;
		}
		public class APIOutputClass {

		}
		public class CallPyApi {
			private readonly StocksContext _context;
			//public bool isNeedUpdate(string stockCode ) {
			//	var q = from o in _context.Stock.ToList()
			//			where o.SnCode == stockCode
			//			orderby o.StDate descending
			//			select o.StDate;
			//	()
			//}
			public async Task<string> UpdateOneStock( string stockCode, string date ,int? port) {
				// 設定 Python 腳本的路徑
				string pythonScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\net8.0\", "").Replace(@"\bin\Release\net8.0\", ""), "wwwroot", "py", "PyAPI.py");
				//Console.WriteLine(pythonScriptPath); 
				// 檢查腳本檔案是否存在
				if (!System.IO.File.Exists(pythonScriptPath)) {
					return "Python script file does not exist." + pythonScriptPath;
				}
				string result = "";
				// 創建 ProcessStartInfo 對象以啟動 Python 解釋器
				ProcessStartInfo startInfo = new ProcessStartInfo();
				
				//startInfo.FileName = "python"; // 假設 Python 已經添加到系統的 PATH 中
				startInfo.FileName = (port==80)?@"C:\Users\-I\AppData\Local\Programs\Python\Python311\python.exe": "python"; // 假設 Python 已經添加到系統的 PATH 中
				result+="目前Port號："+port+"使用python 路徑:" + ((port == 80) ? @"C:\Users\-I\AppData\Local\Programs\Python\Python311\python.exe" : "python")+"\r\n";
				startInfo.Arguments = pythonScriptPath;
				result+="使用python Script:" + pythonScriptPath + "\r\n";
				startInfo.RedirectStandardInput = true;
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;

				// 啟動 Python 解釋器並執行腳本
				using (Process process = Process.Start(startInfo)) {
					// 向 Python 腳本傳遞資料
					using (StreamWriter writer = process.StandardInput) {
						await writer.WriteLineAsync(stockCode);
						await writer.WriteLineAsync(date);
					}

					// 讀取 Python 腳本的輸出
					using (StreamReader reader = process.StandardOutput) {
						
						 result += await reader.ReadToEndAsync()+"\r\n";
						return result;
					}
				}
			}
		}
	}

}
