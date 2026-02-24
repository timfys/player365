using System;

namespace SmartWinners.Models;

public class BeforeLoginVisitedPageInfo
{
    public string PagePath { get; set; } = string.Empty;

    public DateTime UTCDateTime { get; set; }

    public string? Referer { get; set; }
}
