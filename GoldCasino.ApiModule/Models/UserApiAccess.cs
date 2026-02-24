namespace GoldCasino.ApiModule.Models;

public record UserApiAccess(int EntityId, string Username, string Password, int BusinessId = 0, int AffiliateId = 0);