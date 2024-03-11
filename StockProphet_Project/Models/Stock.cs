using System;
using System.Collections.Generic;

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
