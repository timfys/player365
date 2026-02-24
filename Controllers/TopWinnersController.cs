using Microsoft.AspNetCore.Mvc;
using SmartWinners.Models;
using SmartWinners.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartWinners.Controllers;

[Route("")]
public class TopWinnersController(TopWinnersService winnersService) : Controller
{
	[HttpGet("[controller]/[action]")]
	public async Task<IActionResult> GetList([FromQuery(Name = "t")] TopWinnersFilterType type, [FromQuery(Name = "c")] int count, [FromQuery(Name = "lg")] string? lang)
	{
		try
		{
			var winners = await winnersService.GetTopWinners(count, lang ?? "en");

			//var winners = TopWinnersListModel.GetTest(umbracoHelper);

			if (winners is null)
				return new EmptyResult();

			winners.FilterType = type;
			winners.Count = count;
			switch (type)
			{
				case TopWinnersFilterType.Latest:
					winners.List = [.. winners.List.OrderByDescending(x => x.WonDate).Take(count)];
					return PartialView("/Views/Partials/HomePage/_TopWinnersCarousel.cshtml", winners);
				case TopWinnersFilterType.BigWins:
					winners.List = [.. winners.List.OrderByDescending(x => x.ProfitUsd).Take(count)];
					return PartialView("/Views/Partials/HomePage/_TopWinnersCarousel.cshtml", winners);
				case TopWinnersFilterType.TopMultiplayers:
					winners.List = [.. winners.List.OrderByDescending(x => x.Multiplayer).Take(count)];
					return PartialView("/Views/Partials/HomePage/_TopWinnersCarousel.cshtml", winners);
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

		}
		catch (Exception ex)
		{
			// Log the exception (you can use any logging framework you prefer)
			Console.WriteLine($"Error fetching top winners: {ex.Message}");
			return new EmptyResult();
		}
	}

	[HttpGet("[controller]/[action]")]
	public async Task<IActionResult> Get([FromQuery(Name = "t")] TopWinnersFilterType type, [FromQuery(Name = "lg")] string? lang)
	{
		try
		{
			var winners = await winnersService.GetTopWinners(50, lang ?? "en");

			if (winners is null)
				return new EmptyResult();

			IEnumerable<TopWinnersModel> ordered = type switch
			{
				TopWinnersFilterType.Latest => winners.List.OrderByDescending(x => x.WonDate),
				TopWinnersFilterType.BigWins => winners.List.OrderByDescending(x => x.ProfitUsd),
				TopWinnersFilterType.TopMultiplayers => winners.List.OrderByDescending(x => x.Multiplayer),
				_ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
			};

			const int count = 40;
			var topCandidates = ordered.Take(count).ToArray();
			var random = new Random();
			var randomWinner = topCandidates[random.Next(count)];

			return PartialView(
					"/Views/Partials/HomePage/_TopWinnersCarouselItem.cshtml",
					(randomWinner, type)
			);
		}
		catch (Exception ex)
		{
			// Log the exception (you can use any logging framework you prefer)
			Console.WriteLine($"Error fetching top winners: {ex.Message}");
			return new EmptyResult();
		}
	}
	//[HttpGet("[controller]/[action]")]
	//public async Task<IActionResult> Get([FromQuery(Name = "t")] TopWinnersFilterType type)
	//{
	//	var winners = await winnersService.GetTopWinners();

	//	if (winners is null)
	//		return new EmptyResult();

	//	switch (type)
	//	{
	//		case TopWinnersFilterType.Latest:
	//			return PartialView("/Views/Partials/HomePage/_TopWinnersCarouselItem.cshtml", (winners?.List.OrderByDescending(x => x.WonDate).Take(1).First(), type));
	//		case TopWinnersFilterType.BigWins:
	//			return PartialView("/Views/Partials/HomePage/_TopWinnersCarouselItem.cshtml", (winners?.List.OrderByDescending(x => x.ProfitUsd).Take(1).First(), type));
	//		case TopWinnersFilterType.TopMultiplayers:
	//			return PartialView("/Views/Partials/HomePage/_TopWinnersCarouselItem.cshtml", (winners?.List.OrderByDescending(x => x.Multiplayer).Take(1).First(), type));
	//		default:
	//			throw new ArgumentOutOfRangeException(nameof(type), type, null);
	//	}
	//}
}