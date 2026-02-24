using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api;

public class GameTransactionsGetRequest
{
  public string[]? Fields { get; set; }
  public Dictionary<string, string>? Filter { get; set; }
  public int? LimitFrom { get; set; }
  public int? LimitCount { get; set; }
}


// | game_transactionID | integratorID | transaction_Type | gameID | date-created        | entityID | trans_id | round_id | provider_id | match_id | amountUSD | GGR_pct | order_productId | balance | matrix | sync_modified_date  |
public class GameTransaction
{
  [JsonPropertyName("game_transactionID")] public int GameTransactionID { get; set; }
  [JsonPropertyName("transaction_Type")] public int TransactionType { get; set; }
  [JsonPropertyName("integratorID")] public string IntegratorID { get; set; }
  [JsonPropertyName("gameID")] public int GameID { get; set; }
  [JsonPropertyName("entityID")] public int EntityId { get; set; }
  [JsonPropertyName("game_image")] public string GameImage { get; set; }
  [JsonPropertyName("amountUSD")] public decimal AmountUSD { get; set; }
  [JsonPropertyName("GGR_pct")] public decimal GgrPct { get; set; }
  [JsonPropertyName("balance")] public decimal Balance { get; set; }
  [JsonPropertyName("matrix")] public string Matrix { get; set; }
  [JsonPropertyName("round_id")] public string RoundId { get; set; }
  [JsonPropertyName("trans_id")] public string TransId { get; set; }
  [JsonPropertyName("provider_id")] public string ProviderId { get; set; }
  [JsonPropertyName("match_id")] public string MatchId { get; set; }
  [JsonPropertyName("date_created")] public string DateCreated { get; set; }
  [JsonPropertyName("order_productId")] public string OrderProductId { get; set; }
  [JsonPropertyName("sync_modified_date")] public string SyncModifiedDate { get; set; } = string.Empty;
  [JsonPropertyName("ExecuteTime")] public int? ExecuteTime { get; set; }
}

public class GameTransactionGetResult
{
  public List<GameTransaction> Transactions { get; set; } = [];
}