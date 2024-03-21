using Microsoft.AspNetCore.Mvc;
using StockProphet_Project.Models;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace StockProphet_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly StocksContext _context;
		public AdminController(StocksContext stockcontext, IConfiguration configuration)
		{
			_context = stockcontext;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AdminStock()
        {
            List<object> results = new List<object>();
            string connectionString = _configuration.GetConnectionString("StocksConnstring");
            //var query = _context.Stock
            //    .OrderBy(x=>x.SnCode)
            //    .ThenByDescending(x=> x.StDate ).ToList();
            string sqlQuery = "SELECT *\r\nFROM (\r\n    SELECT *,\r\n          " +
                " ROW_NUMBER() OVER (PARTITION BY SN_Code ORDER BY ST_Date DESC) AS RowNum\r\n  " +
                "  FROM stock\r\n) AS RankedStock\r\nWHERE RowNum <= 5;";
            SqlConnection sqlconnect = new SqlConnection(connectionString);
            sqlconnect.Open();
            SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlconnect);
            SqlDataReader reader = sqlCommand.ExecuteReader();


            while (reader.Read())
            {
                // 读取每一行的数据并添加到结果列表中
                results.Add(new
                {
                    SNCode = reader["SN_Code"],
                    SNName = reader["SN_Name"],
                    StDate = reader["St_Date"],
                    SteOpen = reader["Ste_Open"],
                    SteClose = reader["Ste_Close"],
                    SteMax = reader["Ste_Max"],
                    SteMin = reader["Ste_Min"]

                });
            }


            return Json(results);
        }
        public IActionResult AdminMember()
        {
            var query=_context.DbMembers.ToList();
            return Json(query);
        }
        public IActionResult AdminModel()
        {
            var query = _context.DbModels.ToList();
            return Json(query);

        }
    }
}
