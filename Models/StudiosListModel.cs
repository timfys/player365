using System.Collections.Generic;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

namespace SmartWinners.Models;

public class StudiosListModel
{
	public string CarouselName { get; set; }

	public string Title { get; set; }

	public List<Provider> List { get; set; }
}