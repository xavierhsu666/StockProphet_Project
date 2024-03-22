namespace StockProphet_Project.Models
{
    public class PredictData
    {
        public DateTime ST_Date { get; set; }
        public string SN_Code { get; set; }
        public string SN_Name { get; set; }
        public decimal STe_Close { get; set; }
        public int Pid { get; set; }
        public string? Pstock { get; set; }
        public string? Pvariable { get; set; }

        public decimal? Plabel { get; set; }
        public byte? Pprefer { get; set; }

        public string? Paccount { get; set; }

        public DateTime? PbulidTime { get; set; }

        public DateTime? PfinishTime { get; set; }

        public string? Pstatus { get; set; }
    }
}
