namespace StockProphet_Project.Models
{
    public partial class DbMember
    {
        public int Mid { get; set; }

        public string? MaccoMnt { get; set; }

        public string? Mpassword { get; set; }

        public string? MtrueName { get; set; }

        public string? Mgender { get; set; }

        public DateOnly? Mbirthday { get; set; }

        public byte? MinvestYear { get; set; }

        public string? Memail { get; set; }

        public string? Mlevel { get; set; }

        public byte? Mprefer { get; set; }

        public DateOnly? MregisterTime { get; set; }

        public DateTime? MlastLoginTime { get; set; }

        public string? MfavoriteModel { get; set; }
    }
}
