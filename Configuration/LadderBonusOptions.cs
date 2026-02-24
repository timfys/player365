using System.Collections.Generic;

namespace SmartWinners.Configuration;

public class LadderBonusOptions
{
    public const string SectionName = "LadderBonus";

    public List<LadderStepOptions> Steps { get; set; } = [];
}

public class LadderStepOptions
{
    public int Serial { get; set; }
    public string StepText { get; set; } = string.Empty;
    public string BonusPercentageText { get; set; } = string.Empty;
    public decimal MinDepositAmount { get; set; }
    public decimal BonusAddOnAmount { get; set; }
    public decimal Multiplier { get; set; } = 1.0m;
    public string TotalPlayText { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
