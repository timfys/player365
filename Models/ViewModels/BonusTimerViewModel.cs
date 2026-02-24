using System;

namespace SmartWinners.Models.ViewModels;

public class BonusTimerViewModel
{
	public string DisplayText { get; set; } = "00:00:00";
	public DateTime? ExpirationUtc { get; set; }
}