using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockProphet_Project.Models;

public partial class DbModel {
	[Display(Name = "PID")]
	public int Pid { get; set; }
	[Display(Name = "股票代號")]
	public string? Pstock { get; set; }
	[Display(Name = "投入變數")]
	public string? Pvariable { get; set; }
	[Display(Name = "預測價")]
	public decimal? Plabel { get; set; }
	[Display(Name = "投資偏好")]

	public byte? Pprefer { get; set; }
	[Display(Name = "建立預測者")]

	public string? Paccount { get; set; }
	[Display(Name = "建立時間")]

	public DateTime? PbulidTime { get; set; }
	[Display(Name = "結案時間")]

	public DateTime? PfinishTime { get; set; }
	[Display(Name = "狀態")]

	public string? Pstatus { get; set; }
	[Display(Name = "參數及評估值")]

	public string? Dummyblock { get; set; }
	[Display(Name = "使用模型")]

	public string? Pmodel { get; set; }
	[Display(Name = "準確率")]

	public double? PAccuracyRatio { get; set; }
	[Display(Name = "預測前參考價")]

	// 2024/3/30 新增
	public decimal? PreDictLabel { get; set; }
	[Display(Name = "實際價")]
	public decimal? PCurLabel { get; set; }
	[Display(Name = "預期走勢")]
	public string? PreTrend { get; set; }
	[Display(Name = "實際走勢")]
	public string? PActTrend { get; set; }
	[Display(Name = "走勢預測結果")]
	public string? PResult { get; set; }
	[Display(Name = "誤差比例")]
	public double? PSpreadRatio { get; set; }
	[Display(Name = "更新時間")]
	public DateTime? PUpdateTime { get; set; }
}
