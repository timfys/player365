using GoldCasino.ApiModule.HttpClients.Lvslot.Models;
using System.Net.Http.Json;

namespace GoldCasino.ApiModule.HttpClients.Lvslot;
public class LvslotApiClient(HttpClient http, IOptions<LvslotApiOptions> options)
{
	public async Task<OpenGameResponse?> OpenGameAsync(OpenGameRequest request)
	{
		request.Hall = string.IsNullOrEmpty(request.Hall) ? options.Value.HallId : request.Hall;
		request.Key = string.IsNullOrEmpty(request.Key) ? options.Value.HallKey : request.Key;

		request.Cmd = "openGame";

		var resp = await http.PostAsJsonAsync("openGame/", request);
		resp.EnsureSuccessStatusCode();

		return await resp.Content.ReadFromJsonAsync<OpenGameResponse>();
	}
}
