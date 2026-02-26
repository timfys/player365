using GoldCasino.ApiModule.Dtos.Games;
using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.HttpClients.Lvslot;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;
using Microsoft.AspNetCore.Mvc;
using PalaceCasino.Agent.Client;
using SmartWinners.Helpers;
using SmartWinners.Models;
using SmartWinners.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static SmartWinners.Controllers.GamesQueryHelper;

namespace SmartWinners.Controllers;

[Route("")]
public class GamesController(GamesService gamesService, StudiosService studiosService, IGoldslotService goldslotService, IGoldSlotApiClient goldslotApiClient, LvslotApiClient lvslotApiClient, AuthService authService, IBusinessApiService businessApiService, IPlayerClub365ApiService playerClub365ApiService) : Controller
{
	[Route("/game/{gameId}")]
	[Route("/game/{gameId}/{gameName}")]
	[Route("{langIso}/game/{gameId}")]
	[Route("{langIso}/game/{gameId}/{gameName}")]
	public async Task<IActionResult> GameById([FromRoute] string? langIso, [FromRoute] int gameId)
	{
		if (gameId <= 0)
			return NotFound();

		if (string.IsNullOrEmpty(langIso))
			langIso = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

		var game = await gamesService.GetByIdCode(langIso, gameId, IdentityHelper.GetUserIp(HttpContext), HttpContext);
		Provider? provider = null;
		if (game is not null)
		{
			provider = await studiosService.GetById(game.StudioId).ConfigureAwait(false);
			game.Slug = string.IsNullOrEmpty(game.Slug) ? game?.Name is null
				? "" : string.Join("-",
				game.Name
				.ToLowerInvariant()
				.Trim()
				// normalize to spaces first so we can split on them
				.Replace('+', ' ')
				.Replace("&", " and ")
				.Replace('/', ' ')
				.Replace('\\', ' ')
				.Replace('.', ' ')
				.Split(' ', StringSplitOptions.RemoveEmptyEntries))
				// strip remaining characters that commonly break routes or look bad in slugs
				.Replace("?", "")
				.Replace("%", "")
				.Replace("#", "")
				.Replace("\"", "")
				.Replace("'", "") : game.Slug;
			return View("/Views/Game/Index.cshtml", new PlayGameModel(provider, game));
		}

		var providers = await studiosService.Get(0, 1);
		provider = providers.FirstOrDefault();
		return View("/Views/Game/Index.cshtml", new PlayGameModel(provider, null));
	}

	[Route("/games/{categoryId:int}/{categoryName?}")]
	[Route("{langIso}/games/{categoryId:int}/{categoryName?}")]
	public async Task<IActionResult> CategoryIndex(
		[FromRoute] string? langIso,
		[FromRoute] int categoryId,
		[FromRoute] string? categoryName,
		[FromQuery] string? q,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 30,
		[FromQuery] string? sort = "id",     // id | name | provider
		[FromQuery] string? dir = null,    // asc | desc
		[FromQuery] string? device = null
		)
	{
		var principal = HttpContext.User;
		var isAuthorized = principal?.Identity?.IsAuthenticated ?? false;
		var user = isAuthorized ? principal?.ToUserApiAccess() : null;
		langIso ??= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

		var filters = new List<KeyValuePair<string, string>>();

		var cleaned = CleanQuery(q);
		if (!string.IsNullOrEmpty(cleaned))
		{
			// EXAMPLE 1: CONTAINS
			var like = EscapeForSqlLike(cleaned);
			filters.Add(new("pg.game_name", $"LIKE '%{like}%' ESCAPE '\\\\'"));

			// EXAMPLE 2: EXACT (uncomment if needed instead of LIKE)
			// var eq = cleaned.Replace("'", "''");
			// filters.Add(new("pg.game_name", $"= '{eq}'"));

			// EXAMPLE 3: STARTS WITH (uncomment if needed)
			// var sw = EscapeForSqlLike(cleaned);
			// filters.Add(new("pg.game_name", $"LIKE '{sw}%' ESCAPE '\\\\'"));
		}

		var ua = Request?.Headers?.UserAgent.ToString() ?? string.Empty;
		var deviceExpr = DeviceFilterFromQueryOrUa(device ?? string.Empty, ua);
		if (!string.IsNullOrWhiteSpace(deviceExpr))
			filters.Add(new("device", deviceExpr));

		if (isAuthorized)
			filters.Add(new("Hall_Balance", ">0"));

		var prm = new GamesListParameters
		{
			Lang = langIso,
			CategoryId = categoryId,
			CategoryName = categoryName ?? "",
			Query = q ?? "",
			Page = page,
			PageSize = pageSize,
			SortBy = sort?.ToLowerInvariant(),
			SortDir = dir?.ToLowerInvariant(),
			Filters = filters
		};

		if (categoryId is 1 or 2 && isAuthorized)
		{
			if (user is null)
				return Unauthorized();
			prm = prm with { EntityId = user.EntityId, Password = user.Password, Username = user.Username };
		}

		if (categoryId is 1 or 2 && !isAuthorized)
			return View("/Views/Game/List.cshtml", new GamesListModel());

		var model = await gamesService.GetList<GameSimpleDto>(prm);

		return View("/Views/Game/List.cshtml", model);
	}

	[Route("/games/s/all")]
	[Route("{langIso}/games/s/all")]
	public async Task<IActionResult> AllStudios([FromRoute] string? langIso)
	{
		var gameStudios = await studiosService.Get(showAtStudiosOnly: true);

		// Show only root providers to avoid duplicate integrator entries
		gameStudios = gameStudios.Where(s => s.ParentProviderId == 0).ToList();

		return View("/Views/Game/Studios.cshtml", gameStudios);
	}


	[Route("/games/s/{studioId:int}/{studioName?}")]
	[Route("{langIso}/games/s/{studioId:int}/{studioName?}")]
	public async Task<IActionResult> StudioIndex(
		[FromRoute] string? langIso,
		[FromRoute] string? studioName,
		[FromRoute] int studioId,
		[FromQuery] string? q,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 30,
		[FromQuery] string? sort = "id",
		[FromQuery] string? dir = null,
		[FromQuery] string? device = null)
	{
		if (studioId <= 0)
			return NotFound();

		var studio = await studiosService.GetById(studioId);
		if (studio is null)
			return NotFound();

		var providerIds = new List<int> { studio.Id };

		// Include children (or siblings when entering via a child) so users see all related games.
		if (studio.ParentProviderId == 0)
		{
			var children = await studiosService.GetChildren(studio.Id);
			providerIds.AddRange(children.Select(c => c.Id));
		}
		else
		{
			providerIds.Add(studio.ParentProviderId);
			var siblings = await studiosService.GetChildren(studio.ParentProviderId);
			providerIds.AddRange(siblings.Select(c => c.Id));
		}

		langIso ??= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

		var filters = new List<KeyValuePair<string, string>>
		{
			new("pg.provider_id", $"IN ({string.Join(",", providerIds.Distinct())})")
		};

		var cleaned = CleanQuery(q);
		if (!string.IsNullOrEmpty(cleaned))
		{
			var like = EscapeForSqlLike(cleaned);
			filters.Add(new("pg.game_name", $"LIKE '%{like}%' ESCAPE '\\\\'"));
		}

		var ua = Request?.Headers?.UserAgent.ToString() ?? string.Empty;
		var deviceExpr = DeviceFilterFromQueryOrUa(device ?? string.Empty, ua);
		if (!string.IsNullOrWhiteSpace(deviceExpr))
			filters.Add(new("device", deviceExpr));
			
		var parameters = new GamesListParameters
		{
			Lang = langIso,
			CategoryId = 0,
			CategoryName = studioName ?? studio.Name ?? string.Empty,
			Query = q ?? string.Empty,
			Page = page,
			PageSize = pageSize,
			SortBy = sort?.ToLowerInvariant(),
			SortDir = dir?.ToLowerInvariant(),
			Filters = filters
		};

		var games = await gamesService.GetList<GameSimpleDto>(parameters);

		games.StudioGameList = true;
		games.Title = studio.Name ?? string.Empty;
		games.StudioImgUrl = studio.LogoUrl ?? string.Empty;
		games.ProviderId = studio.Id;
		var slug = string.IsNullOrWhiteSpace(studioName)
			? (studio.Name?.ToLowerInvariant().Replace(" ", "-") ?? string.Empty)
			: studioName;
		games.ViewLink = $"/games/s/{studio.Id}/{slug}";

		return View("/Views/Game/List.cshtml", games);
	}

	[HttpGet("/api/games/search")]
	[HttpGet("{langIso}/api/games/search")]
	public async Task<IActionResult> SearchGames(
		[FromRoute] string? langIso,
		[FromQuery] string? name,
		[FromQuery] string? studio,
		[FromQuery] int limit = 30)
	{
		var principal = HttpContext.User;
		var isAuthorized = principal?.Identity?.IsAuthenticated ?? false;
	
		langIso ??= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

		var cleanedName = CleanQuery(name);
		var cleanedStudio = CleanQuery(studio);

		if (string.IsNullOrEmpty(cleanedName) && string.IsNullOrEmpty(cleanedStudio))
			return BadRequest("Provide at least one search parameter (name or studio).");

		if (limit <= 0)
			limit = 30;
		else if (limit > 100)
			limit = 100;

		var filters = new List<KeyValuePair<string, string>>();

		if (!string.IsNullOrEmpty(cleanedName))
		{
			var like = EscapeForSqlLike(cleanedName);
			filters.Add(new("pg.game_name", $"LIKE '%{like}%' ESCAPE '\\\\'"));
		}

		var studioLookup = new Dictionary<int, string>();
		if (!string.IsNullOrEmpty(cleanedStudio))
		{
			var studioTerm = cleanedStudio.Replace("'", "''");
			var studios = await studiosService.Get(0, 25, studioTerm);
			if (studios is null || studios.Count == 0)
				return Ok(new { items = Array.Empty<object>(), total = 0, hasMore = false });

			var studioIds = studios.Select(s => s.Id).Distinct().ToArray();
			if (studioIds.Length == 1)
			{
				filters.Add(new("pg.provider_id", $"= {studioIds[0]}"));
			}
			else
			{
				const int maxProviders = 25;
				var limited = studioIds.Take(maxProviders);
				filters.Add(new("pg.provider_id", $"IN ({string.Join(",", limited)})"));
			}

			foreach (var studioItem in studios)
				studioLookup[studioItem.Id] = studioItem.Name ?? string.Empty;
		}


		if (isAuthorized)
			filters.Add(new("Hall_Balance", ">0"));

		var parameters = new GamesListParameters
		{
			Lang = langIso,
			CategoryId = 0,
			Page = 1,
			PageSize = limit,
			Query = cleanedName ?? cleanedStudio ?? string.Empty,
			Filters = filters
		};

		var gamesList = await gamesService.GetList<GameSearchDto>(parameters);

		var games = gamesList.List ?? [];

		if (games.Count == 0)
			return Ok(new { items = Array.Empty<object>(), total = 0, hasMore = false });

		if (studioLookup.Count == 0)
		{
			var studioIds = games.Select(g => g.StudioId).Distinct().Take(10);
			foreach (var id in studioIds)
			{
				var studioInfo = await studiosService.GetById(id);
				if (studioInfo is not null)
					studioLookup[id] = studioInfo.Name ?? string.Empty;
			}
		}

		var items = games.Select(g => new
		{
			id = g.Id,
			code = g.GameCode,
			name = g.Name,
			imageUrl = g.ImageUrl,
			studioId = g.StudioId,
			studioName = studioLookup.TryGetValue(g.StudioId, out var value) ? value : null
		});

		return Ok(new
		{
			items,
			total = games.Count,
			hasMore = gamesList.HasMore
		});
	}

	[HttpGet("/play/{integratoreId}/{providerId}/{gameId}")]
	[HttpGet("{langIso}/play/{integratoreId}/{providerId}/{gameId}")]
	public async Task<IActionResult> GenerateGameUrl(string? langIso, int integratoreId, int providerId, string gameId)
	{
		if (providerId <= 0)
			return BadRequest("Provider ID must be a positive integer");

		if (string.IsNullOrEmpty(gameId))
			return BadRequest("Game ID must be a non-empty string");

		var principal = HttpContext.User;
		var isAuthorized = principal?.Identity?.IsAuthenticated ?? false;
		var user = principal?.ToUserApiAccess();

		if (!isAuthorized)
			return Redirect($"{langIso}/sign-in?r={langIso}/play/{providerId}/{gameId}");

		if (user is null)
			return Unauthorized();

		var launchResult = await ResolveGameLaunchAsync(langIso, integratoreId, providerId, gameId, user);

		if (!launchResult.Success)
		{
			if (launchResult.TreatAsBadRequest)
				return BadRequest(new { Code = launchResult.ErrorCode, Message = launchResult.ErrorMessage });

			return Ok(new { launchResult.ErrorCode, launchResult.ErrorMessage });
		}

		return Ok(new { redirectUrl = launchResult.RedirectUrl });
	}

	[HttpGet("/game-fullscreen/{gameId:int}/{gameSlug?}")]
	public async Task<IActionResult> GameFullscreen(int gameId, string? gameSlug, [FromQuery] string? lid, [FromQuery] string? langIso)
	{
		if (gameId <= 0)
			return NotFound();

		langIso ??= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
		var game = await gamesService.GetByIdCode(langIso, gameId, HttpContext);
		if (game is null)
			return NotFound();

		var user = HttpContext.User.ToUserApiAccess();
		string? authError = null;
		if (user is null && !string.IsNullOrWhiteSpace(lid))
		{
			var lidResult = await authService.SignInAsync(lid);
			if (lidResult.IsSuccess && int.TryParse(lidResult.Value?.EntityId, out var entityId))
			{
				user = new UserApiAccess(entityId, lidResult.Value!.Username, lidResult.Value.Password);
			}
			else
			{
				authError = lidResult.Error?.Message ?? "Unable to validate the provided lid token.";
			}
		}

		var viewModel = new GameFullscreenViewModel
		{
			GameTitle = game.Name ?? $"Game {game.Id}",
			BackgroundImage = game.ImageUrl ?? game.OgImage
		};

		if (user is null)
		{
			viewModel.ErrorMessage = authError ?? "Authentication required to open this game.";
			return View("/Views/Game/Fullscreen.cshtml", viewModel);
		}

		if (string.IsNullOrWhiteSpace(game.GameCode))
		{
			viewModel.ErrorMessage = "Selected game is missing a playable code.";
			return View("/Views/Game/Fullscreen.cshtml", viewModel);
		}

		var launchResult = await ResolveGameLaunchAsync(langIso, game.IntegratoreId, game.StudioId, game.GameCode, user);

		if (!launchResult.Success)
		{
			viewModel.ErrorMessage = launchResult.ErrorMessage ?? "Unable to load the requested game.";
			return View("/Views/Game/Fullscreen.cshtml", viewModel);
		}

		viewModel.RedirectUrl = launchResult.RedirectUrl;
		return View("/Views/Game/Fullscreen.cshtml", viewModel);
	}

	private async Task SetHallBalanceToZeroAsync(string gameId)
	{
		if (!int.TryParse(gameId, out var gameIdValue))
			return;

		await playerClub365ApiService.GamesUpdateAsync(new GamesUpdateRequest
		{
			GameId = gameIdValue,
			HallBalance = 0
		});
	}

	private async Task<GameLaunchResult> ResolveGameLaunchAsync(string? langIso, int integratoreId, int providerId, string gameId, UserApiAccess user)
	{
		if (string.IsNullOrWhiteSpace(gameId))
			return new GameLaunchResult(false, null, "invalid_game", "Game ID must be a non-empty string.", true);

		//TODO: move to config
		if (integratoreId == 3)
		{
			var resultA = await lvslotApiClient.OpenGameAsync(new()
			{
				GameId = gameId,
				Language = langIso ?? "en",
				Login = user.EntityId.ToString(),
				Domain = "https://www.playerclub365.com",
				ExitUrl = $"https://{HttpContext.Request.Host}",
				Demo = "0"
			});

			if (resultA is null || resultA.Status != "success" || resultA.Content?.Game is null || string.IsNullOrEmpty(resultA.Content.Game.Url))
			{
				if (resultA?.Error?.Contains("hall_balance_less_100") ?? false)
				{
					await SetHallBalanceToZeroAsync(gameId);
					await businessApiService.OutgoingAddAsync(new()
					{
						EntityId = user.EntityId,
						EntityMobile = user.Username,
						EntityName = user.Username,
						GameName = "Unknown",
						GameId = gameId,
						GameUrl = $"/play/{integratoreId}/{providerId}/{gameId}",
						Timestamp = DateTimeOffset.UtcNow,
					});
				}

				return new GameLaunchResult(false, null, resultA?.Status ?? "lvslot_error", resultA?.Error ?? "Unable to resolve game session.", true);
			}

			// Validate that the returned URL is actually a valid URL (not an error message like "Game is closed")
			if (!Uri.TryCreate(resultA.Content.Game.Url, UriKind.Absolute, out var lvslotUri) ||
				(lvslotUri.Scheme != Uri.UriSchemeHttp && lvslotUri.Scheme != Uri.UriSchemeHttps))
			{
				return new GameLaunchResult(false, null, "game_closed", resultA.Content.Game.Url ?? "Game temporarily closed.", false);
			}

			return new GameLaunchResult(true, resultA.Content.Game.Url, null, null, false);
		}

		var userCode = await goldslotService.GetUserCodeAsync($"{user.EntityId}");

		var result = await goldslotApiClient.GameUrlAsync(new()
		{
			User_code = userCode,
			Provider_id = providerId,
			Game_symbol = gameId,
			Return_url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}",
			Win_ratio = 0,
			Lang = ReqGameUrlLang._1
		});

		if (result.Data is null || string.IsNullOrEmpty(result.Data.Game_url))
			return new GameLaunchResult(false, null, result.Code.ToString(), result.Message, false);

		// Validate that the returned URL is actually a valid URL (not an error message like "Game is closed")
		if (!Uri.TryCreate(result.Data.Game_url, UriKind.Absolute, out var goldslotUri) ||
			(goldslotUri.Scheme != Uri.UriSchemeHttp && goldslotUri.Scheme != Uri.UriSchemeHttps))
		{
			return new GameLaunchResult(false, null, "game_closed", result.Data.Game_url ?? "Game temporarily closed.", false);
		}

		return new GameLaunchResult(true, result.Data.Game_url, null, null, false);
	}
[HttpPost("/api/game/check-frame")]
public async Task<IActionResult> CheckFrameUrl([FromBody] FrameCheckRequest request)
{
    if (request is null || string.IsNullOrWhiteSpace(request.Url))
        return BadRequest(new { hasError = true, message = "Invalid URL" });

    try
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false // ВАЖНО
        };

        using var httpClient = new HttpClient(handler);
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
        );

        var response = await httpClient.GetAsync(request.Url);

        var statusCode = (int)response.StatusCode;
        if (request.Url.Contains("play.aleaplay") || request.Url.Contains("HTTP500") || (response.Headers?.Location?.ToString().Contains("play.aleaplay") ?? false) || (response.Headers?.Location?.ToString().Contains("HTTP500") ?? false))
        {
	        return Ok(new
	        {
	            hasError = true,
	            message = $"error",
	        });
        }


        // // 🔹 2️⃣ Если неуспешный статус
        // if (!response.IsSuccessStatusCode)
        // {
        //     return Ok(new
        //     {
        //         hasError = true,
        //         message = $"HTTP {statusCode}",
        //         statusCode
        //     });
        // }
        //
        // // 🔹 3️⃣ Если 200 — читаем HTML
        // var html = await response.Content.ReadAsStringAsync();
        //
        // if (html.Contains("error", StringComparison.OrdinalIgnoreCase) ||
        //     html.Contains("exception", StringComparison.OrdinalIgnoreCase) ||
        //     html.Contains("game closed", StringComparison.OrdinalIgnoreCase))
        // {
        //     return Ok(new
        //     {
        //         hasError = true,
        //         message = "Game provider returned error content",
        //         statusCode
        //     });
        // }

        return Ok(new
        {
            hasError = false,
            message = $"ok {request.Url} {response.Headers?.Location?.ToString()}",
        });
    }
    catch (Exception ex)
    {
        return Ok(new
        {
            hasError = true,
            message = ex.Message
        });
    }
}



	private sealed record GameLaunchResult(bool Success, string? RedirectUrl, string? ErrorCode, string? ErrorMessage, bool TreatAsBadRequest);

}
public class FrameCheckRequest
{
	public string Url { get; set; } = string.Empty;
}