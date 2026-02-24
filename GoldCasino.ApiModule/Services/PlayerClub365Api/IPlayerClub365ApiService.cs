using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api;

public interface IPlayerClub365ApiService
{
  Task<Result<GameTransactionGetResult, Error>> GameTransactionsGetAsync(GameTransactionsGetRequest request, UserApiAccess? userApiAccess = null);
  Task<Result<GameTransactionUpdateResult, Error>> GameTransactionUpdateAsync(GameTransactionUpdateRequest request, UserApiAccess? userApiAccess = null);
  Task<Result<GamesGetResult, Error>> GamesGetAsync(GamesGetRequest request);
  Task<Result<GamesUpdateResult, Error>> GamesUpdateAsync(GamesUpdateRequest request, UserApiAccess? userApiAccess = null);
  Task<Result<ProvidersGetResult, Error>> ProvidersGetAsync(ProvidersGetRequest request);
  Task<Result<LanguagesGetResult, Error>> LanguagesGetAsync(LanguagesGetRequest request, UserApiAccess? userApiAccess = null);
  Task<Result<string, Error>> EntityBonusesUpdateAsync(EntityBonusesUpdate request);
  Task<Result<EntityBonusesGetResult, Error>> EntityBonusesGetAsync(UserApiAccess accessData);
}
