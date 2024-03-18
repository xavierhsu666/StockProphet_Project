using Microsoft.AspNetCore.Mvc;
using StockProphet_Project.Models;
using System.Net.Mail;

namespace StockProphet_Project.Controllers
{
	public class MemberController : Controller
	{
		//在MemberController中可以讀取該StockProphet資料
		private readonly StocksContext _context;
		public MemberController(StocksContext context)
		{
			_context = context;
		}

		//會員主頁
		public IActionResult Index()
		{
			return View();
		}

		//註冊頁面
		public IActionResult Register()
		{
			return View();
		}
		//檢查帳戶名
		[HttpGet]
		public bool RegisterA(string MAccoMnt)
		{
			//判斷帳戶名是否有重複            
			var result = _context.DbMembers.Where(x => x.MaccoMnt == MAccoMnt);
			return result.Any();
		}

		//註冊頁面-新會員註冊OK
		[HttpPost]
		public IActionResult Register(string MAccoMnt, string MPassword, string MEmail, string MTrueName, DateOnly MBirthday, string MGender, byte MInvestYear, string MLevel, string registerTime)
		{
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
			_context.DbMembers.Add(new DbMember
			{
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
		public IActionResult Login()
		{
			return View();
		}
		//判斷登入會員等級並決定可看到頁面的權限
		[HttpGet]
		public string checkLogin(string MAccoMnt, string MPassword)
		{
			//var memberAccoMnt = _context.Members.FirstOrDefault(x => x.MaccoMnt == MAccoMnt);
			//var memberEmail = _context.Members.FirstOrDefault(x=>x.Memail == MAccoMnt);

			var member = _context.DbMembers.FirstOrDefault(x => x.MaccoMnt == MAccoMnt);
			//先檢查是否存在該會員
			//if (memberAccoMnt != null || memberEmail != null)
			if (member != null)
			{
				//再判斷密碼是否正確
				//if (memberAccoMnt.Mpassword == MPassword || memberEmail.Mpassword == MPassword)
				if (member.Mpassword == MPassword)
				{
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
					switch (member.Mlevel)
					{
						case "一般會員":
							return "我是一般會員";

						case "高級會員":
							return "我是高級會員";

						case "管理者":
							return "我是管理者";

						default:
							return "一般訪客";
					}
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

		//忘記密碼頁forgot-password
		public IActionResult ForgotPassword()
		{
			return View();
		}

		//檢查忘記密碼頁面的信箱是否正確
		[HttpGet]
		public bool CheckEmail(string MEmail)
		{
			System.Diagnostics.Debug.WriteLine("test" + MEmail);
			var result = _context.DbMembers.Where(x => x.Memail == MEmail);
			return result.Any();
		}

		[HttpGet]
		//系統自動發驗證碼信件
		public void sendGmail(string MEmail)
		{
			var member = _context.DbMembers.FirstOrDefault(x => x.Memail == MEmail);
			var result = member.Memail;
			System.Diagnostics.Debug.WriteLine("顯示的" + result);

			if (member != null)
			{
				MailMessage mail = new MailMessage();
				//                          前面是發信的email  後面是顯示的名稱   
				mail.From = new MailAddress("j1129w@gmail.com", "測試自動寄信功能");
				//收件者email
				mail.To.Add("boris83418@gmail.com");//result
													//設定優先權
				mail.Priority = MailPriority.Normal;
				//標題
				mail.Subject = "StockProphet_驗證碼發送，該信箱為系統自動發信，請勿回信";
				//內容
				mail.Body = "<h1>StockProphet系統自動發信_驗證碼發送</h1>\r\n" +
							"<p>以下是您本次修改密碼的驗證碼</p>\r\n" +
							"<h3>驗證碼:</h3>\r\n<h3><b>測試中</b></h3>\r\n";
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

			}
			else
			{
				System.Diagnostics.Debug.WriteLine("顯示" + "郵件未寄送");

			}
		}

		//比對驗證碼
		public bool CheckVerifyIdentifyCode(string VerifyIdentifyCode)
		{
			System.Diagnostics.Debug.WriteLine("test1" + VerifyIdentifyCode);
			var result = _context.DbMembers.Where(x => x.Memail == VerifyIdentifyCode);
			return result.Any();
		}
		//未登入時的修改密碼頁面      
		public IActionResult RevisePassword()
		{
			return View();
		}
		//儲存修改後的密碼



		//管理者頁面Admin
		public IActionResult Admin()
		{
			return View();
		}



	}
}
