using GoldCasino.ApiModule.Integrations.PlayerClub365;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi.Policies;
using GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api;

internal class PlayerClub365ApiService(
  IOptions<Playerclub365Options> options,
  IPlayerclub365 client,
  ILogger<PlayerClub365ApiService> logger) : SoapServiceBase, IPlayerClub365ApiService
{
  public async Task<Result<GameTransactionGetResult, Error>> GameTransactionsGetAsync(GameTransactionsGetRequest request, UserApiAccess? access = null)
  {
    var req = BuildGameTransactionRequest(request, access);

    var raw = await ExecuteAsync<List<GameTransaction>>(
        async () => (await client.Game_Transactions_GetAsync(req)).@return);

    if (!raw.IsSuccess) return Result<GameTransactionGetResult, Error>.Fail(raw.Error!);

    return Result<GameTransactionGetResult, Error>.Ok(
      new() { Transactions = raw.Value! });
  }

  public async Task<Result<GamesGetResult, Error>> GamesGetAsync(GamesGetRequest request)
  {
    var req = BuildGamesGetRequest(request);

    var raw = await ExecuteAsync<List<Game>>(
        async () => (await client.Games_GetAsync(req)).@return);

    if (!raw.IsSuccess) return Result<GamesGetResult, Error>.Fail(raw.Error!);

    return Result<GamesGetResult, Error>.Ok(
      new() { Games = raw.Value! });
  }

  public async Task<Result<GamesUpdateResult, Error>> GamesUpdateAsync(GamesUpdateRequest request, UserApiAccess? access = null)
  {
    var req = BuildGamesUpdateRequest(request, access);

    if (req.NamesArray.Length == 0)
      return Result<GamesUpdateResult, Error>.Ok(new() { Games = [] });

    var raw = await ExecuteAsync<List<Game>>(
        async () => (await client.Games_UpdateAsync(req)).@return);

    if (!raw.IsSuccess) return Result<GamesUpdateResult, Error>.Fail(raw.Error!);

    return Result<GamesUpdateResult, Error>.Ok(
      new() { Games = raw.Value! });
  }


  public async Task<Result<ProvidersGetResult, Error>> ProvidersGetAsync(ProvidersGetRequest request)
  {
    var req = BuildProvidersGetRequest(request);

    var raw = await ExecuteAsync<List<Provider>>(
        async () => (await client.Providers_GetAsync(req)).@return);

    if (!raw.IsSuccess) return Result<ProvidersGetResult, Error>.Fail(raw.Error!);

    return Result<ProvidersGetResult, Error>.Ok(
      new() { Providers = raw.Value! });
  }

  public async Task<Result<LanguagesGetResult, Error>> LanguagesGetAsync(LanguagesGetRequest request, UserApiAccess? access = null)
  {
    var req = BuildLanguagesGetRequest(request, access);

    var raw = await ExecuteAsync<List<Language>>(
        async () => (await client.Languages_GetAsync(req)).@return);

    if (!raw.IsSuccess) return Result<LanguagesGetResult, Error>.Fail(raw.Error!);

    return Result<LanguagesGetResult, Error>.Ok(
      new() { Languages = raw.Value! });
  }

  public async Task<Result<string, Error>> EntityBonusesUpdateAsync(EntityBonusesUpdate request)
  {
    var (names, values) = FlattenDtoToSoapArrays(request, SoapUpdatePolicies.Default);

    var master = options.Value.Credentials;

    var soapRequest = new Entity_Bonuses_UpdateRequest
    {
      Ol_EntityId = master.EntityId,
      Ol_Username = master.Username,
      Ol_Password = master.Password,
      recordId = request.RecordId,
      EntityId = request.EntityId,
      BonusType = (int)request.BonusType,
      Serial = request.Serial,
      Value = Convert.ToDouble(request.Value),
      NamesArray = names,
      ValuesArray = values
    };

    var raw = await ExecuteRawAsync(async () => (await client.Entity_Bonuses_UpdateAsync(soapRequest)).@return);

    return Result<string, Error>.Ok(raw);
  }

  public async Task<Result<EntityBonusesGetResult, Error>> EntityBonusesGetAsync(UserApiAccess accessData)
  {
    var req = new Entity_Bonuses_GetRequest
    {
      Ol_EntityId = accessData.EntityId,
      Ol_Username = accessData.Username,
      Ol_Password = accessData.Password
    };

    var raw = await ExecuteAsync<EntityBonusesGetResult>(
        async () => (await client.Entity_Bonuses_GetAsync(req)).@return);

    if (!raw.IsSuccess) return Result<EntityBonusesGetResult, Error>.Fail(raw.Error!);

    return Result<EntityBonusesGetResult, Error>.Ok(raw.Value!);
  }

  public Task<Result<GameTransactionUpdateResult, Error>> GameTransactionUpdateAsync(GameTransactionUpdateRequest request, UserApiAccess? userApiAccess = null)
  {
    throw new NotImplementedException();
  }

  // public async Task<Result<GameTransactionUpdateResult, Error>> GameTransactionUpdateAsync(GameTransactionUpdateRequest request, UserApiAccess? access = null)
  // {
  //   var req = BuildGameTransactionUpdateRequest(request, access);

  //   var raw = await ExecuteAsync<List<Entity>>(
  //     async () => (await client.Game_Transaction_UpdateAsync(req)).@return);

  //   if (!raw.IsSuccess) return Result<GameTransactionUpdateResult, Error>.Fail(raw.Error!);

  //   return Result<GameTransactionUpdateResult, Error>.Ok(
  //     new() { Transactions = raw.Value! });
  // }

  #region Request Builders
  private Game_Transactions_GetRequest BuildGameTransactionRequest(
          GameTransactionsGetRequest model,
          UserApiAccess? access)
  {
    var master = options.Value.Credentials;

    var req = new Game_Transactions_GetRequest
    {
      Ol_EntityId = access?.EntityId ?? master.EntityId,
      Ol_Username = access?.Username ?? master.Username,
      Ol_Password = access?.Password ?? master.Password,
      LimitCount = model.LimitCount ?? 0,
      LimitFrom = model.LimitFrom ?? 0,
    };

    // — filters -----------------------------------------------------------
    if (model.Filter?.Count > 0)
    {
      req.FilterFields = [.. model.Filter.Keys];
      req.FilterValues = [.. model.Filter.Values];
    }

    // — sample fields ------------------------------------------------------
    req.Fields = model.Fields is { Length: > 0 }
                  ? model.Fields
                  : ["integratorID", "gameID", "amountUSD", "entityID"];

    return req;
  }

  public static Providers_GetRequest BuildProvidersGetRequest(ProvidersGetRequest model)
  {
    var req = new Providers_GetRequest
    {
      Lang_Code = model.LangCode,
      LimitCount = model.LimitCount ?? 0,
      LimitFrom = model.LimitFrom ?? 0,
    };

    // — filters -----------------------------------------------------------
    if (model.Filter?.Count > 0)
    {
      req.FilterFields = [.. model.Filter.Keys];
      req.FilterValues = [.. model.Filter.Values];
    }

    // — sample fields ------------------------------------------------------
    req.Fields = model.Fields is { Length: > 0 }
                  ? model.Fields
                  : ["game_name"];

    return req;
  }

  public static Games_GetRequest BuildGamesGetRequest(GamesGetRequest model)
  {
    var req = new Games_GetRequest
    {
      Ol_EntityId = model.EntityId ?? 0,
      Ol_Username = model.Username ?? string.Empty,
      Ol_Password = string.IsNullOrWhiteSpace(model.Password) ? "FreeRun86" : model.Password,
      Lang_Code = model.LangCode,
      LimitCount = model.LimitCount ?? 0,
      LimitFrom = model.LimitFrom ?? 0,
    };

    // — filters -----------------------------------------------------------
    if (model.Filter?.Count > 0)
    {
      req.FilterFields = [.. model.Filter.Keys];
      req.FilterValues = [.. model.Filter.Values];
    }

    // — sample fields ------------------------------------------------------
    req.Fields = model.Fields is { Length: > 0 }
                  ? model.Fields
                  : ["game_name"];

    return req;
  }

  private Games_UpdateRequest BuildGamesUpdateRequest(GamesUpdateRequest model, UserApiAccess? access)
  {
    var master = options.Value.Credentials;
    string[] names;
    string[] values;
    if (model.Fields is { Count: > 0 })
    {
      names = new string[model.Fields.Count];
      values = new string[model.Fields.Count];
      var idx = 0;

      foreach (var pair in model.Fields)
      {
        names[idx] = pair.Key;
        values[idx] = pair.Value;
        idx++;
      }
    }
    else
    {
      (names, values) = FlattenDtoToSoapArrays(model, SoapUpdatePolicies.Default);
    }

    return new Games_UpdateRequest
    {
      Ol_EntityId = access?.EntityId ?? master.EntityId,
      Ol_Username = access?.Username ?? master.Username,
      Ol_Password = access?.Password ?? master.Password,
      gameId = model.GameId,
      NamesArray = names,
      ValuesArray = values
    };
  }

  private Languages_GetRequest BuildLanguagesGetRequest(
          LanguagesGetRequest model,
          UserApiAccess? access)
  {
    var master = options.Value.Credentials;

    var req = new Languages_GetRequest
    {
      Ol_EntityId = access?.EntityId ?? master.EntityId,
      Ol_Username = access?.Username ?? master.Username,
      Ol_Password = access?.Password ?? master.Password,
      LimitCount = model.LimitCount ?? 0,
      LimitFrom = model.LimitFrom ?? 0,
    };

    if (model.Filter?.Count > 0)
    {
      req.FilterFields = [.. model.Filter.Keys];
      req.FilterValues = [.. model.Filter.Values];
    }

    req.Fields = model.Fields is { Length: > 0 }
                  ? model.Fields
                  : ["languageISO", "name", "name_english", "created_date", "last_scan"];

    return req;
  }

  #endregion
}
