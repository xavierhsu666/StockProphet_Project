using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using StockProphet_Project.Models;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using Tensorflow;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace StockProphet_Project.Controllers {
	public class MemberController : Controller {
		//在MemberController中可以讀取該StockProphet資料
		private readonly IConfiguration _configuration;
		private readonly StocksContext _context;
		public MemberController( StocksContext context , IConfiguration configuration) {
			_context = context;
			_configuration = configuration;
		}

		//會員主頁
		public IActionResult Index() {
			return View();
		}
		//修改個人資料頁面
		public IActionResult Edit() {
			return View();
		}
		//確認舊密碼是否正確
		public bool checkPassword( string checkPassword ) {
			System.Diagnostics.Debug.WriteLine("舊密碼:" + checkPassword);

			//判斷該會員的舊密碼是否一致            
			HttpContext.Session.GetString("MID");
			System.Diagnostics.Debug.WriteLine("登入會員的Id:" + HttpContext.Session.GetString("MID"));
			var member = _context.DbMembers.Where(x => x.Mid.ToString() == HttpContext.Session.GetString("MID"));
			var result = member.Where(x => x.Mpassword == checkPassword);
			System.Diagnostics.Debug.WriteLine("測試會員" + member);

			return result.Any();
		}
		// //儲存修改後的會員資料
		[HttpPut]
		public bool SaveReviseMemberData( string Account, string NewPassword, string NewEmail, string NewName, string NewInvestYear ) {
			//Console.WriteLine( NewPassword==null);
			//Console.WriteLine("NewEmail:" + NewEmail);
			//Console.WriteLine("NewName:" + NewName);
			//Console.WriteLine("NewInvestYear:" + NewInvestYear);
			var query = _context.DbMembers.SingleOrDefault(x => x.MaccoMnt == Account);

			if (NewPassword == null) {
				//不修改DbMember中的密碼
				query.Memail = NewEmail;
				query.MtrueName = NewName;
				query.MinvestYear = Convert.ToByte(NewInvestYear);
				_context.SaveChanges();

			} else {
				//修改DbMember中的密碼
				query.Memail = NewPassword;
				query.Memail = NewEmail;
				query.MtrueName = NewName;
				query.MinvestYear = Convert.ToByte(NewInvestYear);
				_context.SaveChanges();
			}

			return true;

        }

			return View();
		}
		[HttpGet]
		public IActionResult MyPredictResultBoris(string customername)
		{
			List<object> results = new List<object>();
			string connectionString = _configuration.GetConnectionString("StocksConnstring");
			string sqlQuery = $@"SELECT B.Pid, B.PAccount, B.Pstock, B.Plabel, B.dummyblock, 
                        B.PBulidTime, B.Pfinishtime, A.ST_Date, A.ste_Close 
                        FROM DB_model AS B 
                        OUTER APPLY (
                            SELECT TOP 5 *
                            FROM Stock
                            WHERE SN_code = B.Pstock
                                  AND ST_Date <= B.PBulidTime
                            ORDER BY ST_Date DESC
                        ) AS A 
                        WHERE B.PAccount = '{customername}'";
			Console.WriteLine(sqlQuery);
			SqlConnection sqlconnect = new SqlConnection(connectionString);
			sqlconnect.Open();
			SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlconnect);
			SqlDataReader reader = sqlCommand.ExecuteReader();


			while (reader.Read())
			{

				results.Add(new
				{
					STdate = reader["ST_Date"],
					PAcount = reader["PAccount"],
					PStock = reader["Pstock"],
					PLabel = reader["Plabel"],
					Parameter = reader["dummyblock"],
					PBuildTime = reader["PBulidTime"],
					PFinsihTime = reader["Pfinishtime"],
					SteClose= reader["Ste_Close"]

				});
			}


			return Json(results);
		}

		//我的預測結果
		//沛棋繪製卡片的功能

		////網址傳資料|回傳預測內容
		//public IActionResult showPredictions(string id)
		//{

		//    var viewModel = _context.DbModels.ToList();
		//    var query = from p in viewModel
		//                where p.Paccount == id
		//                select new
		//                {
		//                    Account = p.Paccount,
		//                    Variable = p.Pvariable,
		//                    Label = p.Plabel,
		//                    FinishTime = Convert.ToDateTime(p.PfinishTime).ToString("yyyy-MM-dd")
		//                };

		//    return Json(query);
		//}
		//如果該會員預測過10檔股票，就要跑10次!!!
		//網址傳資料|該股票所有內容(for預測用
		//public IActionResult showAllStocks(string id)
		//{
		//    var viewModel = _context.Stock.ToList();
		//    var query = from p in viewModel
		//                where p.SnCode == id
		//                select new
		//                {
		//                    Date = p.StDate,
		//                    Close = p.SteClose,
		//                    StockName = p.SnName
		//                };

		//    return Json(query);
		//}

		//註冊頁面-2		
		public IActionResult Register() {
			return View();
		}
		//檢查帳戶名
		[HttpGet]
		public bool RegisterA( string MAccoMnt ) {
			//判斷帳戶名是否有重複            
			var result = _context.DbMembers.Where(x => x.MaccoMnt == MAccoMnt);
			return result.Any();
		}

		//註冊頁面-新會員註冊OK
		[HttpPost]
		public IActionResult Register( string MAccoMnt, string MPassword, string MEmail, string MTrueName, DateOnly MBirthday, string MGender, byte MInvestYear, string MLevel, string registerTime ) {
			//System.Diagnostics.Debug.WriteLine(MAccoMnt);
			//System.Diagnostics.Debug.WriteLine(MPassword);
			//System.Diagnostics.Debug.WriteLine(MEmail);
			//System.Diagnostics.Debug.WriteLine(MTrueName);
			//System.Diagnostics.Debug.WriteLine(MBirthday);
			//System.Diagnostics.Debug.WriteLine(MGender);
			//System.Diagnostics.Debug.WriteLine(MInvestYear);
			//System.Diagnostics.Debug.WriteLine(MLevel);

			//轉換日期格式 字串->DateOnly
			DateOnly CurrentTime = DateOnly.Parse("2024-03-14");
			DateOnly MregisterTime = DateOnly.Parse(CurrentTime.ToString("yyyy-MM-dd"));
			System.Diagnostics.Debug.WriteLine(MregisterTime);


			//將資料存到資料庫
			_context.DbMembers.Add(new DbMember {
				MaccoMnt = MAccoMnt,
				Mpassword = MPassword,
				Memail = MEmail,
				MtrueName = MTrueName,
				Mbirthday = MBirthday,
				Mgender = MGender,
				MinvestYear = MInvestYear,
				Mlevel = MLevel,
				MregisterTime = MregisterTime
			});
			_context.SaveChanges();
			return View();
		}

		//會員登入頁
		public IActionResult Login() {
			return View();
		}
		//判斷登入會員等級並決定可看到頁面的權限
		[HttpGet]
		public string checkLogin( string MAccoMnt, string MPassword ) {
			Console.WriteLine("測試登入帳號資料" + MAccoMnt);
			if (MAccoMnt.IndexOf('@') >= 0) {
				var member = _context.DbMembers.FirstOrDefault(x => x.Memail == MAccoMnt);
				Console.WriteLine(member);
				//先檢查是否存在該會員
				if (member != null) {
					//再判斷密碼是否正確
					//if (memberAccoMnt.Mpassword == MPassword || memberEmail.Mpassword == MPassword)
					if (member.Mpassword == MPassword) {
						//Session傳值
						HttpContext.Session.SetString("MID", member.Mid.ToString());
						HttpContext.Session.SetString("MEmail", member.Memail!);
						HttpContext.Session.SetString("Mlevel", member.Mlevel!);
						HttpContext.Session.SetString("MaccoMnt", member.MaccoMnt!);
						HttpContext.Session.SetString("Mlevel", member.Mlevel!);
						//不常用_供修改會員資料頁面使用
						HttpContext.Session.SetString("MtrueName", member.MtrueName!);
						HttpContext.Session.SetString("Mbirthday", value: member.Mbirthday.ToString()!);
						HttpContext.Session.SetString("Mgender", member.Mgender!);
						HttpContext.Session.SetString("MinvestYear", member.MinvestYear.ToString()!);

						//Console.WriteLine(member.MinvestYear);
						//Console.WriteLine(member.Mlevel);

						//依照會員身分給予不同權限的頁面
						switch (member.Mlevel) {
							case "一般會員":
								return "一般會員";

							case "高級會員":
								return "高級會員";

							case "管理者":
								return "管理者";

							default:
								return "一般訪客";
						}
					} else {
						return "輸入的密碼不正確";
					}
				} else {
					return "會員名稱錯誤";
				}
			} else {
				var member = _context.DbMembers.FirstOrDefault(x => x.MaccoMnt == MAccoMnt);
				Console.WriteLine(member);
				//先檢查是否存在該會員
				if (member != null) {
					//再判斷密碼是否正確
					//if (memberAccoMnt.Mpassword == MPassword || memberEmail.Mpassword == MPassword)
					if (member.Mpassword == MPassword) {
						//Session傳值
						HttpContext.Session.SetString("MID", member.Mid.ToString());
						HttpContext.Session.SetString("MEmail", member.Memail!);
						HttpContext.Session.SetString("Mlevel", member.Mlevel!);
						HttpContext.Session.SetString("MaccoMnt", member.MaccoMnt!);
						HttpContext.Session.SetString("Mlevel", member.Mlevel!);
						//不常用_供修改會員資料頁面使用
						HttpContext.Session.SetString("MtrueName", member.MtrueName!);
						HttpContext.Session.SetString("Mbirthday", value: member.Mbirthday.ToString()!);
						HttpContext.Session.SetString("Mgender", member.Mgender!);
						HttpContext.Session.SetString("MinvestYear", member.MinvestYear.ToString()!);

						//Console.WriteLine(member.MinvestYear);
						//Console.WriteLine(member.Mlevel);

						//依照會員身分給予不同權限的頁面
						switch (member.Mlevel) {
							case "一般會員":
								return "一般會員";

							case "高級會員":
								return "高級會員";

							case "管理者":
								return "管理者";

							default:
								return "一般訪客";
						}
					} else {
						return "輸入的密碼不正確";
					}
				} else {
					return "會員名稱錯誤";
				}
			}


			//var member = _context.DbMembers.FirstOrDefault(x => x.MaccoMnt == MAccoMnt);
			//         Console.WriteLine(member);
			//         //先檢查是否存在該會員
			//         if (member != null)
			//         {
			//             //再判斷密碼是否正確
			//             //if (memberAccoMnt.Mpassword == MPassword || memberEmail.Mpassword == MPassword)
			//             if (member.Mpassword == MPassword)
			//             {                    
			//                 //Session傳值
			//                 HttpContext.Session.SetString("MID", member.Mid.ToString());
			//                 HttpContext.Session.SetString("MEmail", member.Memail!);
			//                 HttpContext.Session.SetString("Mlevel", member.Mlevel!);
			//                 HttpContext.Session.SetString("MaccoMnt", member.MaccoMnt!);
			//                 HttpContext.Session.SetString("Mlevel", member.Mlevel!);
			//                 //不常用_供修改會員資料頁面使用
			//                 HttpContext.Session.SetString("MtrueName", member.MtrueName!);
			//                 HttpContext.Session.SetString("Mbirthday", value: member.Mbirthday.ToString()!);
			//                 HttpContext.Session.SetString("Mgender", member.Mgender!);
			//                 HttpContext.Session.SetString("MinvestYear", member.MinvestYear.ToString()!);

			//		//Console.WriteLine(member.MinvestYear);
			//		//Console.WriteLine(member.Mlevel);

			//                 //依照會員身分給予不同權限的頁面
			//                 switch (member.Mlevel)
			//                 {
			//                     case "一般會員":
			//                         return "一般會員";

			//                     case "高級會員":
			//                         return "高級會員";

			//                     case "管理者":
			//                         return "管理者";

			//			default:
			//				return "一般訪客";
			//		}
			//	}
			//	else
			//	{
			//		return "輸入的密碼不正確";
			//	}
			//}
			//else
			//{
			//	return "會員名稱錯誤";
			//}

		}

		//忘記密碼頁forgot-password
		public IActionResult ForgotPassword() {
			return View();
		}

		//檢查忘記密碼頁面的信箱是否正確
		[HttpGet]
		public bool CheckEmail( string MEmail ) {
			System.Diagnostics.Debug.WriteLine("test" + MEmail);
			var result = _context.DbMembers.Where(x => x.Memail == MEmail);

			if (result.Any() == true) {
				var resultmid = _context.DbMembers.FirstOrDefault(x => x.Memail == MEmail).Mid;
				//抓取忘記密碼的會員Id
				var memberId = resultmid;
				System.Diagnostics.Debug.WriteLine("顯示忘記密碼的會員ID" + memberId);

			}

			return result.Any();
		}

		[HttpGet]
		//系統自動發驗證碼信件
		public void sendGmail( string MEmail, int verifyCode ) {
			var member = _context.DbMembers.FirstOrDefault(x => x.Memail == MEmail);
			var result = member.Memail;
			System.Diagnostics.Debug.WriteLine("顯示的" + result);

			if (member != null) {
				MailMessage mail = new MailMessage();
				//                          前面是發信的email  後面是顯示的名稱   
				mail.From = new MailAddress("j1129w@gmail.com", "系統驗證碼發送");
				//收件者email
				mail.To.Add(MEmail);//result\
				//mail.To.Add("wryi636@gmail.com");//result\
				//mail.To.Add("boris83418@gmail.com");//result
				//設定優先權
				mail.Priority = MailPriority.Normal;
				//標題
				mail.Subject = "StockProphet_身分驗證，此為系統自動發信，請勿回信";
				//內容
				mail.Body = "<h1>StockProphet系統 驗證碼</h1>\r\n" +
							"<p>以下是您本次修改密碼的驗證碼</p>\r\n" +
							"<h3>驗證碼:</h3>\r\n" +
							"<h3>" + verifyCode + "</h3>\r\n" ;
				//內容使用html
				mail.IsBodyHtml = true;
				//設定gmail的smtp(這是google的)
				SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587);
				//在gmail的帳號密碼
				MySmtp.Credentials = new System.Net.NetworkCredential("j1129w", "dcxnogpcooeynevi");
				//開啟ssl
				MySmtp.EnableSsl = true;
				//發送郵件
				MySmtp.Send(mail);
				//放掉宣告出來的MySetp
				MySmtp = null;
				//放掉宣告出來的mail
				mail.Dispose();
				System.Diagnostics.Debug.WriteLine("顯示" + "郵件發送成功");
			} else {
				System.Diagnostics.Debug.WriteLine("顯示" + "郵件未寄送");
			}
		}

		//比對驗證碼
		public bool CheckVerifyIdentifyCode( string MEmail, string VerifyIdentifyCode ) {
			System.Diagnostics.Debug.WriteLine("比對忘記密碼會員的_Email:" + MEmail);
			System.Diagnostics.Debug.WriteLine("比對忘記密碼會員的_驗證碼:" + VerifyIdentifyCode);

			var query = _context.DbMembers.FirstOrDefault(x => x.Memail == MEmail);
			var result = query.Memail;
			if (result == VerifyIdentifyCode) {
				return true;
			} else {
				return false;
			}
		}

		//未登入時的修改密碼頁面      
		public IActionResult RevisePassword() {
			return View();
		}
		////儲存修改後的密碼
		[HttpPut]
		public bool SaveRevisePassword( string ForgotMemberEmail, string RevisePassword ) {
			System.Diagnostics.Debug.WriteLine("儲存修改後的密碼_Email:" + ForgotMemberEmail);
			System.Diagnostics.Debug.WriteLine("儲存修改後的密碼_Password:" + RevisePassword);

			var query = _context.DbMembers.FirstOrDefault(x => x.Memail == ForgotMemberEmail);
			if (query != null) {
				//修改DbMember中的密碼
				query.Mpassword = RevisePassword;
				_context.SaveChanges();
			}
			return true;
		}


		//管理者頁面Admin
		public IActionResult Admin() {
			return View();
		}



	}
}
