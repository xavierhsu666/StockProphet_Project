using System;
using System.Collections.Generic;

namespace StockProphet_Project.Models;

public partial class DbModel
{
    public int Pid { get; set; }

    public string? Pstock { get; set; }

    public string? Pvariable { get; set; }

    public decimal? Plabel { get; set; }

    public byte? Pprefer { get; set; }

    public string? Paccount { get; set; }

    public DateTime? PbulidTime { get; set; }

    public DateTime? PfinishTime { get; set; }

    public string? Pstatus { get; set; }

    public string? Dummyblock { get; set; }
	public string? Pmodel { get; set; }
	public double? PAccuracyRatio { get; set; }
}
