using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

namespace SmartWinners.Models;

public class PlayGameModel(Provider? studio, Game? game)
{
	public Provider? Studio { get; } = studio;
	public Game? Game { get; } = game;
}