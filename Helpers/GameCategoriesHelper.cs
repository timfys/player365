using SmartWinners.Configuration;
using SmartWinners.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartWinners.Helpers;

public class GameCategoriesHelper
{
	private static readonly CasinoGamesApiConfiguration _config = EnvironmentHelper.CasinoGamesApiConfiguration;
	public async static Task<List<IGrouping<int, GameCategory>>> GetGroupedCategoriesAsync()
	{
		var client = _config.InitClient();
		var response = await client.Game_Categories_GetAsync(new()
		{
			Ol_EntityId = _config.ol_EntityId,
			Ol_Username = _config.ol_UserName,
			Ol_Password = _config.ol_Password,
			Lang_Code = "en",
			Fields = ["category_name", "is_dynamic", "enabled"]
		});
			
		var json = response.@return;
		var categories = JsonSerializer.Deserialize<List<GameCategory>>(json) ?? [];

		return [.. categories
				.Where(c => c.Enabled == 1)
				.OrderBy(c => c.IsDynamic)
				.GroupBy(c => c.IsDynamic)];
	}
}
