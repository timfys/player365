using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using System.Collections.Generic;

namespace SmartWinners.Models;

public class GamesListModel
{
	public string Title { get; set; }

	public List<Game> List { get; set; }

	public string ViewLink { get; set; } = "/games/s/all";

	public int CategoryID { get; set; }

	public bool StudioGameList { get; set; } = false;
	public int? ProviderId { get; set; }
	public string? StudioImgUrl { get; set; }
	public string? Query { get; set; }
	public int Page { get; set; } = 1;
	public int PageSize { get; set; }
	public string? SortBy { get; set; }
	public string? SortDir { get; set; }
	public bool HasMore { get; set; }
}
