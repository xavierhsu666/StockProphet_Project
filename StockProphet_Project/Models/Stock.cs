using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StockProphet_Project.Models;

public partial class Stock
{
	[Key]
	[Display(Name = "PK")]
	public string SPk { get; set; } = null!;

	[Display(Name = "日期")]
	public DateOnly StDate { get; set; }

	[Display(Name = "年季")]
	public string? StYearQuarter { get; set; }

	[Display(Name = "季")]
	public string? StQuarter { get; set; }

	[Display(Name = "年")]
	public string? StYear { get; set; }

	[Display(Name = "股票代號")]
	public string SnCode { get; set; } = null!;

	[Display(Name = "股票名稱")]
	public string? SnName { get; set; }

	[Display(Name = "開盤價")]
	public decimal? SteOpen { get; set; }

	[Display(Name = "收盤價")]
	public decimal? SteClose { get; set; }

	[Display(Name = "最高價")]
	public decimal? SteMax { get; set; }

	[Display(Name = "最低價")]
	public decimal? SteMin { get; set; }

	[Display(Name = "震幅")]
	[DisplayFormat(DataFormatString = "{0:0.00}")]
	public double? SteSpreadRatio { get; set; }

	[Display(Name = "交易額")]
	public long? SteTradeMoney { get; set; }

	[Display(Name = "交易量")]
	public int? SteTradeQuantity { get; set; }

	[Display(Name = "交易動作")]
	public int? SteTransActions { get; set; }

	[Display(Name = "拆分年")]
	public byte? SteDividendYear { get; set; }

	[Display(Name = "營收")]
	public decimal? SbYield { get; set; }

	[Display(Name = "PBR比例")]
	public double? SbPbratio { get; set; }

	[Display(Name = "EPS")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SbEps { get; set; }

	[Display(Name = "業內收入")]
	public long? SbBussinessIncome { get; set; }

	[Display(Name = "五日移動平均")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiMovingAverage5 { get; set; }

	[Display(Name = "三十日移動平均")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiMovingAverage30 { get; set; }

	[Display(Name = "五日RSV")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiRsv5 { get; set; }

	[Display(Name = "三十日RSV")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiRsv30 { get; set; }

	[Display(Name = "五日K")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiK5 { get; set; }

	[Display(Name = "三十日K")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiK30 { get; set; }

	[Display(Name = "五日D")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiD5 { get; set; }

	[Display(Name = "三十日D")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiD30 { get; set; }

	[Display(Name = "長期EMA")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiLongEma { get; set; }

	[Display(Name = "短期EMA")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiShortEma { get; set; }

	[Display(Name = "DIF")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiDif { get; set; }

	[Display(Name = "MACD")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiMacd { get; set; }

	[Display(Name = "OSC")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiOsc { get; set; }

	[Display(Name = "PE")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]
    public double? SiPe { get; set; }

	[Display(Name = "MA")]
    [DisplayFormat(DataFormatString = "{0:0.00}")]	
    public double? SiMa { get; set; }
	[Display(Name = "上次更新時間")]
	public DateOnly StUpdateDate { get; set; }
}
public class StockDB
{
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
	public decimal STe_Close { get; set; }
	[Display(Name = "開盤價")]
	public decimal STe_Open { get; set; }
	[Display(Name = "最高價")]
	public decimal STe_Max { get; set; }
	[Display(Name = "最低價")]
	public decimal STe_Min { get; set; }
	[Display(Name = "震幅")]
	public double STe_SpreadRatio { get; set; }
	[Display(Name = "交易額")]
	public long STe_TradeMoney { get; set; }
	[Display(Name = "交易量")]
	public int STe_TradeQuantity { get; set; }
	[Display(Name = "交易動作")]
	public int STe_TransActions { get; set; }
	[Display(Name = "拆分年")]
	public byte STe_Dividend_Year { get; set; }
	[Display(Name = "營收")]
	public decimal SB_Yield { get; set; }
	[Display(Name = "PBR比例")]
	public double SB_PBRatio { get; set; }
	[Display(Name = "EPS")]
	public double SB_EPS { get; set; }
	[Display(Name = "業內收入")]
	public long SB_BussinessIncome { get; set; }
	[Display(Name = "五日移動平均")]
	public double SI_MovingAverage_5 { get; set; }
	[Display(Name = "三十日移動平均")]
	public double SI_MovingAverage_30 { get; set; }
	[Display(Name = "五日RSV")]
	public double SI_RSV_5 { get; set; }
	[Display(Name = "三十日RSV")]
	public double SI_RSV_30 { get; set; }
	[Display(Name = "五日K")]
	public double SI_K_5 { get; set; }
	[Display(Name = "三十日K")]
	public double SI_K_30 { get; set; }
	[Display(Name = "五日D")]
	public double SI_D_5 { get; set; }
	[Display(Name = "三十日D")]
	public double SI_D_30 { get; set; }
	[Display(Name = "長期EMA")]
	public double SI_LongEMA { get; set; }
	[Display(Name = "短期EMA")]
	public double SI_ShortEMA { get; set; }
	[Display(Name = "DIF")]
	public double SI_Dif { get; set; }
	[Display(Name = "MACD")]
	public double SI_MACD { get; set; }
	[Display(Name = "OSC")]
	public double SI_OSC { get; set; }
	[Display(Name = "PE")]
	public double SI_PE { get; set; }
	[Display(Name = "MA")]
	public double SI_MA { get; set; }
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
	public Single STe_TradeMoney { get; set; }
	[Display(Name = "交易量")]
	public Single STe_TradeQuantity { get; set; }
	[Display(Name = "交易動作")]
	public Single STe_TransActions { get; set; }
	[Display(Name = "拆分年")]
	public Single STe_Dividend_Year { get; set; }
	[Display(Name = "營收")]
	public Single SB_Yield { get; set; }
	[Display(Name = "PBR比例")]
	public Single SB_PBRatio { get; set; }
	[Display(Name = "EPS")]
	public Single SB_EPS { get; set; }
	[Display(Name = "業內收入")]
	public Single SB_BussinessIncome { get; set; }
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
