using System;
using System.Collections.Generic;

namespace SmartWinners.Models;

public class BannerWinnersModel
{
    public List<BannerWinners> List { get; set; }
}

public class BannerWinners
{
    public decimal UsdAmount { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
}