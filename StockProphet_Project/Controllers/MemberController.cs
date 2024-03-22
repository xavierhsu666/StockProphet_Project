using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using OneOf.Types;
using StockProphet_Project.Models;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Tensorflow;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Security.Principal;

namespace StockProphet_Project.Controllers {
	public class MemberController : Controller {
		//在MemberController中可以讀取該StockProphet資料
		private readonly StocksContext _context;
		public MemberController(StocksContext context) {
			_context = context;
		}

		//會員主頁-1
		public IActionResult Index() {
			return View();
		}
		//修改個人資料頁面_1-1
		public IActionResult Edit() {
			return View();
		}
		//確認舊密碼是否正確
		public bool checkPassword(string checkPassword) {
			System.Diagnostics.Debug.WriteLine("舊密碼:" + checkPassword);

			//判斷該會員的舊密碼是否一致            
			HttpContext.Session.GetString("MID");
			System.Diagnostics.Debug.WriteLine("登入會員的Id:" + HttpContext.Session.GetString("MID"));
			var member = _context.DbMembers.Where(x => x.Mid.ToString() == HttpContext.Session.GetString("MID"));
			var result = member.Where(x => x.Mpassword == checkPassword);
			System.Diagnostics.Debug.WriteLine("測試會員" + member);

			return result.Any();
		}
		//儲存修改後的會員資料
		[HttpPut]
		public bool SaveReviseMemberData(string Account, string NewPassword, string NewEmail, string NewName, string NewInvestYear) {
			//Console.WriteLine( NewPassword==null);
			//Console.WriteLine("NewEmail:" + NewEmail);
			//Console.WriteLine("NewName:" + NewName);
			//Console.WriteLine("NewInvestYear:" + NewInvestYear);
			var query = _context.DbMembers.SingleOrDefault(x => x.MaccoMnt == Account);
			if (query != null)
			{
				if (NewPassword == null) {
					//不修改DbMember中的密碼
					query.Memail = NewEmail;
					query.MtrueName = NewName;
					query.MinvestYear = Convert.ToByte(NewInvestYear);
					_context.SaveChanges();



				} else {
					//修改DbMember中的密碼
					query.Mpassword = NewPassword;
					query.Memail = NewEmail;
					query.MtrueName = NewName;
					query.MinvestYear = Convert.ToByte(NewInvestYear);
					_context.SaveChanges();
				}

				return true;

			}
			else
			{
				return false;
			}


		}
		//讀取更新後的會員資料
		[HttpGet]
		public string ReturnReviseMemberData(string LogAccount) {
			var query = _context.DbMembers.SingleOrDefault(x => x.MaccoMnt == LogAccount);
			var ReviseMemberData = new
			{
				MtrueName = query.MtrueName,
				MinvestYear = query.MinvestYear,
				Memail = query.Memail
			};
			return ReviseMemberData.ToString()!;
		}

		//我的收藏頁面-菀琇
		public IActionResult MyCollect()
		{

			// 從 Session 中獲取 MID
			var sessionMID = HttpContext.Session.GetString(SessionKeys.MID);

			// 將 MID 轉換為整數
			if (int.TryParse(sessionMID, out int mid))
			{
				// 查詢 MID 對應的資料列
				var item = _context.DbMembers.FirstOrDefault(item => item.Mid == mid);

				if (item != null)
				{


					// 將 MfavoriteModel 的值整理成字串，以空白分隔數字
					string formattedMfavoriteModel = string.Empty; // 若為 NULL，則設為空字串

					if (!string.IsNullOrEmpty(item.MfavoriteModel))
					{

						formattedMfavoriteModel = item.MfavoriteModel.Trim('{', '}'); // 先移除大括號

						// 檢查是否為空字串，若是則直接將 pidStrings.Length 設為 0
						if (formattedMfavoriteModel == "")
						{
							ViewBag.FormattedMfavoriteModel = "尚無收藏項目";
							ViewBag.PidStringsCount = 0;
						}


						else
						{

							var pidStrings = formattedMfavoriteModel.Split(',');

							// 將字串轉換為整數並存儲在列表中
							var pidList = new List<int>();
							foreach (var pidString in pidStrings)
							{
								if (int.TryParse(pidString, out int pid))
								{
									pidList.Add(pid);
								}
							}

							// 將數字陣列傳遞到 View
							ViewBag.FavoriteNumbers = pidList;
							ViewBag.PidStringsCount = pidStrings.Length;


							// 使用 LINQ 查詢 DbModels 資料表，找出符合條件的資料列
							var favoriteItems = _context.DbModels.Where(model => pidList.Contains(model.Pid)).ToList();

							// 將查詢結果傳遞到 View
							ViewBag.FavoriteItems = favoriteItems;



						}
					}
					else
					{
						ViewBag.FormattedMfavoriteModel = "尚無收藏項目"; // 若找不到對應的資料，則設為空字串
						ViewBag.FavoriteNumbers = new int[0]; // 若找不到對應的資料，則設為空陣列
					}
				}
			}
			else
			{
				ViewBag.FormattedMfavoriteModel = "尚無收藏資料";
				ViewBag.FavoriteNumbers = new int[0];

			}

			return View();
		}

		public static class SessionKeys
		{
			public const string MID = "MID";
			// 在這裡添加其他您想要使用的 Session 鍵
		}

		//我的預測結果頁面_1-3
		public IActionResult MyPredictResult() {

			return View();
		}

		//我的預測結果-繪製卡片的功能

		//抓取會員預測過的結果
		public IActionResult MemberPredictData(string LogAccount) {
			//找出登入的會員共有幾筆預測資料

			var query = from m in _context.DbModels
						where m.Paccount == LogAccount
						orderby m.PbulidTime descending
						select new { PID = m.Pid, Paccount = m.Paccount, Pvariable = m.Pvariable, Plabel = m.Plabel, PbulidTime = m.PbulidTime, PfinishTime = m.PfinishTime };
			return Json(query);
		}

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
		public bool RegisterA(string MAccoMnt) {
			//判斷帳戶名是否有重複            
			var result = _context.DbMembers.Where(x => x.MaccoMnt == MAccoMnt);
			return result.Any();
		}

		//註冊頁面-新會員註冊OK
		[HttpPost]
		public IActionResult Register(string MAccoMnt, string MPassword, string MEmail, string MTrueName, DateOnly MBirthday, string MGender, byte MInvestYear, string MLevel, string registerTime) {
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

		//會員登入頁-3
		public IActionResult Login() {
			return View();
		}

		//判斷登入會員等級並決定可看到頁面的權限
		[HttpGet]
		public string checkLogin(string MAccoMnt, string MPassword)
		{
			//Console.WriteLine("測試登入帳號資料" + MAccoMnt);

			//輸入的帳號是信箱
			if (MAccoMnt.IndexOf('@') >= 0)
			{
				var member = _context.DbMembers.FirstOrDefault(x => x.Memail == MAccoMnt);
				Console.WriteLine(member);

				//先檢查是否存在該會員

				if (member != null)
				{
					//再判斷密碼是否正確
					if (member.Mpassword == MPassword)
					{
						//2.包成Json傳值

						var LogMember = new {
							Mid = member.Mid,
							MaccoMnt = member.MaccoMnt,
							MtrueName = member.MtrueName,
							Mgender = member.Mgender,
							Mbirthday = member.Mbirthday,
							MinvestYear = member.MinvestYear,
							Memail = member.Memail,
							Mlevel = member.Mlevel,
							Mprefer = member.Mprefer,
							MregisterTime = member.MregisterTime,
							MfavoriteModel = member.MfavoriteModel };

						//Newtonsoft.Json序列化
						string jsonData1 = JsonConvert.SerializeObject(LogMember);
						Console.WriteLine("登入後會員的資料:" + jsonData1);

						return jsonData1;
					}
					else
					{
						return "輸入的密碼不正確";
					}
				}
				else
				{
					return "會員名稱錯誤";
				}
			}
			//輸入的帳號是帳號名

			else
			{
				var member = _context.DbMembers.FirstOrDefault(x => x.MaccoMnt == MAccoMnt);
				Console.WriteLine(member);
				Console.WriteLine("登入會員的ID" + member.Mid);

				//先檢查是否存在該會員

				if (member != null)
				{
					//再判斷密碼是否正確					
					if (member.Mpassword == MPassword)
					{
						//2.包成Json傳值

						var LogMember = new
						{
							Mid = member.Mid,
							MaccoMnt = member.MaccoMnt,
							MtrueName = member.MtrueName,
							Mgender = member.Mgender,
							Mbirthday = member.Mbirthday,
							MinvestYear = member.MinvestYear,
							Memail = member.Memail,
							Mlevel = member.Mlevel,
							Mprefer = member.Mprefer,
							MregisterTime = member.MregisterTime,
							MfavoriteModel = member.MfavoriteModel
						};

						//Newtonsoft.Json序列化
						string jsonData1 = JsonConvert.SerializeObject(LogMember);
						Console.WriteLine("登入後會員的資料:" + jsonData1);

						return jsonData1;
					}

					else
					{
						return "輸入的密碼不正確";
					}
				}
				else
				{
					return "會員名稱錯誤";
				}
			}
		}

        //判斷從哪個頁面進入到登入畫面
  //      [HttpGet]
		//public bool RedirectToPrevPage(string )
		//{

		//}

        //忘記密碼頁forgot-password-4
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
		//系統自動發驗證碼信件(驗證碼亂數產生)
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
				//mail.To.Add("wryi636@gmail.com");//嘉澤信箱
				mail.To.Add("boris83418@gmail.com");//柏樺信箱
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

		//未登入時的修改密碼頁面-5      
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


		//管理者頁面Admin-6
		public IActionResult Admin() {
			return View();
		}	

    }
}
