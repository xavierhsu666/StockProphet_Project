using System.ComponentModel.DataAnnotations;

namespace StockProphet_Project.Models
{
	public class DbMember
    {
        public int Mid { get; set; }

		[Display(Name = "帳號")]
		public string? MaccoMnt { get; set; }

		[Display(Name = "密碼")]
		public string? Mpassword { get; set; }
		[Display(Name = "真名")]
		public string? MtrueName { get; set; }

		[Display(Name = "性別")]
		public string? Mgender { get; set; }

		[Display(Name = "生日")]
		public DateOnly? Mbirthday { get; set; }

		[Display(Name = "投資年數")]
		public byte? MinvestYear { get; set; }

		[Display(Name = "信箱")]
		public string? Memail { get; set; }

		[Display(Name = "會員等級")]
		public string? Mlevel { get; set; }
       
        
		[Display(Name = "會員偏好")]
		public byte? Mprefer { get; set; }


		[Display(Name = "註冊時間")]
		public DateOnly? MregisterTime { get; set; }

        [Display(Name = "最後登入時間")]
        public DateTime? MlastLoginTime { get; set; }
        [Display(Name = "會員收藏模型")]
        public string? MfavoriteModel { get; set; }
	}

    public class SearchRequestModel
    {
        public string SearchTerm { get; set; }
    }
}
