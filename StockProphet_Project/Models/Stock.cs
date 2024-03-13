using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StockProphet_Project.Models;

public partial class Stock
{
    public string SPk { get; set; } = null!;

    public DateOnly StDate { get; set; }

    public string? StYearQuarter { get; set; }

    public string? StQuarter { get; set; }

    public string? StYear { get; set; }

    public string SnCode { get; set; } = null!;

    public string? SnName { get; set; }

    public decimal? SteOpen { get; set; }

    public decimal? SteClose { get; set; }

    public decimal? SteMax { get; set; }

    public decimal? SteMin { get; set; }

    public double? SteSpreadRatio { get; set; }

    public long? SteTradeMoney { get; set; }

    public int? SteTradeQuantity { get; set; }

    public int? SteTransActions { get; set; }

    public byte? SteDividendYear { get; set; }

    public decimal? SbYield { get; set; }

    public double? SbPbratio { get; set; }

    public double? SbEps { get; set; }

    public long? SbBussinessIncome { get; set; }

    public double? SiMovingAverage5 { get; set; }

    public double? SiMovingAverage30 { get; set; }

    public double? SiRsv5 { get; set; }

    public double? SiRsv30 { get; set; }

    public double? SiK5 { get; set; }

    public double? SiK30 { get; set; }

    public double? SiD5 { get; set; }

    public double? SiD30 { get; set; }

    public double? SiLongEma { get; set; }

    public double? SiShortEma { get; set; }

    public double? SiDif { get; set; }

    public double? SiMacd { get; set; }

    public double? SiOsc { get; set; }

    public double? SiPe { get; set; }

    public double? SiMa { get; set; }
}
public class StockDBA {
	// 根據SCRIPT，新的
	[Key]
	[Display(Name = "PK")]
	public string S_PK { get; set; }
	[Display(Name = "日期")]
	public DateTime ST_Date { get; set; }
	[Display(Name = "年季")]
	public string ST_Year_Quarter { get; set; }
	[Display(Name = "季")]
	public string ST_Quarter { get; set; }
	[Display(Name = "年")]
	public string ST_Year { get; set; }
	[Display(Name = "股票代號")]
	public string SN_Code { get; set; }
	[Display(Name = "股票名稱")]
	public string SN_Name { get; set; }
	[Display(Name = "收盤價")]
	public Single STe_Close { get; set; }
	[Display(Name = "開盤價")]
	public Single STe_Open { get; set; }
	[Display(Name = "最高價")]
	public Single STe_Max { get; set; }
	[Display(Name = "最低價")]
	public Single STe_Min { get; set; }
	[Display(Name = "震幅")]
	public Single STe_SpreadRatio { get; set; }
	[Display(Name = "交易額")]
	public long STe_TradeMoney { get; set; }
	[Display(Name = "交易量")]
	public int STe_TradeQuantity { get; set; }
	[Display(Name = "交易動作")]
	public int STe_TransActions { get; set; }
	[Display(Name = "拆分年")]
	public Single STe_Dividend_Year { get; set; }
	[Display(Name = "營收")]
	public Single SB_Yield { get; set; }
	[Display(Name = "PBR比例")]
	public Single SB_PBRatio { get; set; }
	[Display(Name = "EPS")]
	public Single SB_EPS { get; set; }
	[Display(Name = "業內收入")]
	public long SB_BussinessIncome { get; set; }
	[Display(Name = "五日移動平均")]
	public Single SI_MovingAverage_5 { get; set; }
	[Display(Name = "三十日移動平均")]
	public Single SI_MovingAverage_30 { get; set; }
	[Display(Name = "五日RSV")]
	public Single SI_RSV_5 { get; set; }
	[Display(Name = "三十日RSV")]
	public Single SI_RSV_30 { get; set; }
	[Display(Name = "五日K")]
	public Single SI_K_5 { get; set; }
	[Display(Name = "三十日K")]
	public Single SI_K_30 { get; set; }
	[Display(Name = "五日D")]
	public Single SI_D_5 { get; set; }
	[Display(Name = "三十日D")]
	public Single SI_D_30 { get; set; }
	[Display(Name = "長期EMA")]
	public Single SI_LongEMA { get; set; }
	[Display(Name = "短期EMA")]
	public Single SI_ShortEMA { get; set; }
	[Display(Name = "DIF")]
	public Single SI_Dif { get; set; }
	[Display(Name = "MACD")]
	public Single SI_MACD { get; set; }
	[Display(Name = "OSC")]
	public Single SI_OSC { get; set; }
	[Display(Name = "PE")]
	public Single SI_PE { get; set; }
	[Display(Name = "MA")]
	public Single SI_MA { get; set; }

}
