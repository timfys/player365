// using Microsoft.AspNetCore.Mvc;

// namespace GoldCasino.ApiModule.Controllers.Api;

// [Route("api/[controller]")]
// [ApiController]
// public class GamesController(IGoldslotService goldslotService, IGoldslotApiClient goldslotApiClient) : ControllerBase
// {
// 	[HttpGet("play/{providerId}/{gameId}")]
// 	public async Task<IActionResult> GenerateGameUrl(int providerId, string gameId)
// 	{
// 		if (providerId <= 0)
// 			return BadRequest("Provider ID must be a positive integer");

// 		if (string.IsNullOrEmpty(gameId))
// 			return BadRequest("Game ID must be a non-empty string");

// 		var user = WebStorageUtility.GetSignedUser();
// 		if (user is null)
// 			return Redirect($"{langIso}/sign-in?r={langIso}/play/{providerId}/{gameId}");

// 		var userCode = await goldslotService.GetUserCodeAsync($"{user.EntityId}");

// 		var result = await goldslotApiClient.GameUrlAsync(new()
// 		{
// 			User_code = userCode,
// 			Provider_id = providerId,
// 			Game_symbol = gameId,
// 			Return_url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}",
// 			Win_ratio = 0,
// 			Lang = ReqGameUrlLang._1
// 		});

// 		return Redirect(result.Data.Game_url);
// 	}
// }
