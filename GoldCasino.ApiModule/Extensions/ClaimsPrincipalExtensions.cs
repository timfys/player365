using GoldCasino.ApiModule.Models;
using System.Security.Claims;

namespace GoldCasino.ApiModule.Extensions;
public static class ClaimsPrincipalExtensions
{
	public static UserApiAccess? ToUserApiAccess(this ClaimsPrincipal user)
	{
		if (user?.Identity is not { IsAuthenticated: true })
			return null;

		var entityId = int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var e) ? e : 0;
		var username = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
		var password = user.FindFirst("Password")?.Value ?? string.Empty;
		var businessId = int.TryParse(user.FindFirst("BusinessId")?.Value, out var b) ? b : 0;
		var affiliateId = int.TryParse(user.FindFirst("AffiliateId")?.Value, out var a) ? a : 0;

		return new UserApiAccess(entityId, username, password, businessId, affiliateId);
	}
}
