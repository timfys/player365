namespace SmartWinners.Models.ViewModels;

public enum BannerType
{
  Claim = 1,
  Claimed,
  Locked
}

public class BonusTicketViewModel
{
  public string StepText { get; set; }
  public string BonusPercentageText { get; set; }
  public decimal MinDepositAmount { get; set; }
  public decimal BonusAddOnAmount { get; set; }
  public string PlayWithText { get; set; }
  public string TotalPlayText { get; set; }
  public BannerType Type { get; set; }
  public string ImageUrl { get; set; }
  public int RecordId { get; set; }
  public int Serial { get; set; }
  public string CollectUrl { get; set; }
}