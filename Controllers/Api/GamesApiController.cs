using GoldCasino.ApiModule.Dtos.Games;
using GoldCasino.ApiModule.Extensions;
using Microsoft.AspNetCore.Mvc;
using SmartWinners.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SmartWinners.Controllers.GamesQueryHelper;

namespace SmartWinners.Controllers.Api;

[ApiController]
[Route("api/games")]
public class GamesApiController(GamesService gamesService, StudiosService studiosService) : ControllerBase
{

	[HttpGet("categories/{categoryId:int}/{categoryName?}")]
	[HttpGet("{langIso}/categories/{categoryId:int}/{categoryName?}")]
	public async Task<IActionResult> GetCategoryGames(
		[FromRoute] string? langIso,
		[FromRoute] int categoryId,
		[FromRoute] string? categoryName,
		[FromQuery] string? q,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 30,
		[FromQuery] string? sort = "id",
		[FromQuery] string? dir = null,
		[FromQuery] string? device = null)
	{
		var principal = HttpContext.User;
		var isAuthorized = principal?.Identity?.IsAuthenticated ?? false;
		var user = isAuthorized ? principal?.ToUserApiAccess() : null;
		langIso ??= Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;

		var filters = new List<KeyValuePair<string, string>>();

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

		if (isAuthorized)
			filters.Add(new("Hall_Balance", ">0"));

		var parameters = new GamesListParameters
		{
			Lang = langIso,
			CategoryId = categoryId,
			CategoryName = categoryName ?? string.Empty,
			Query = q ?? string.Empty,
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

			parameters = parameters with { EntityId = user.EntityId, Password = user.Password, Username = user.Username };
		}

		if (categoryId is 1 or 2 && !isAuthorized)
			return Unauthorized();

		var model = await gamesService.GetList<GameSimpleDto>(parameters).ConfigureAwait(false);

		return Ok(model);
	}

	[HttpGet("studios/{studioId:int}/{studioName?}")]
	[HttpGet("{langIso}/studios/{studioId:int}/{studioName?}")]
	public async Task<IActionResult> GetStudioGames(
		[FromRoute] string? langIso,
		[FromRoute] int studioId,
		[FromRoute] string? studioName,
		[FromQuery] string? q,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 30,
		[FromQuery] string? sort = "id",
		[FromQuery] string? dir = null,
		[FromQuery] string? device = null)
	{
		var principal = HttpContext.User;
		var isAuthorized = principal?.Identity?.IsAuthenticated ?? false;

		if (studioId <= 0)
			return NotFound();

		var studio = await studiosService.GetById(studioId).ConfigureAwait(false);
		if (studio is null)
			return NotFound();

		var providerIds = new List<int> { studio.Id };

		// Include children (or siblings when entering via a child) so API consumers get all related games.
		if (studio.ParentProviderId == 0)
		{
			var children = await studiosService.GetChildren(studio.Id).ConfigureAwait(false);
			providerIds.AddRange(children.Select(c => c.Id));
		}
		else
		{
			providerIds.Add(studio.ParentProviderId);
			var siblings = await studiosService.GetChildren(studio.ParentProviderId).ConfigureAwait(false);
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

		if (isAuthorized)
			filters.Add(new("Hall_Balance", ">0"));

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

		var games = await gamesService.GetList<GameSimpleDto>(parameters).ConfigureAwait(false);

		games.StudioGameList = true;
		games.Title = studio.Name ?? string.Empty;
		games.StudioImgUrl = studio.LogoUrl ?? string.Empty;
		var slug = string.IsNullOrWhiteSpace(studioName)
			? (studio.Name?.ToLowerInvariant().Replace(" ", "-") ?? string.Empty)
			: studioName;
		games.ViewLink = $"/games/s/{studio.Id}/{slug}";

		return Ok(games);
	}

}
