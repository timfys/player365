using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartWinners.Helpers;
using System.Threading.Tasks;

namespace SmartWinners.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController(IPlayerClub365ApiService playerClub365ApiService) : ControllerBase
{

	[Authorize]
	[HttpGet("balance/poll")]
	public async Task<IActionResult> BalancePoll()
	{
		var user = HttpContext.User.ToUserApiAccess();

		var bal = await PaymentHelper.GetUserBalance(user, true);

		// Bonus balance - always fetch fresh and cache
		decimal bonusBalance = 0;
		var bonusResult = await playerClub365ApiService.EntityBonusesGetAsync(user);
		if (bonusResult.IsSuccess)
		{
			bonusBalance = bonusResult.Value.BonusBalance;
			PaymentHelper.CacheUserBonusBalance(HttpContext, bonusBalance);
		}

		var totalBalanceUSD = (bal?.BalanceUSD ?? 0) + bonusBalance;

		return Ok(new { balanceUSD = totalBalanceUSD, balanceLocal = bal?.BalanceLocal ?? 0, bonusBalance });
	}
}
