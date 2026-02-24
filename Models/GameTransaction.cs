using System;
using Newtonsoft.Json;

namespace SmartWinners.Models;

public class GameTransaction
{
    [JsonProperty("PlayedDate")] public DateTime PlayedDate { get; set; }

    [JsonProperty("Name")] public string Name { get; set; }

    [JsonProperty("betUSD")] public decimal BetUSD { get; set; }

    [JsonProperty("balanceUSD")] public decimal BalanceUSD { get; set; }

    [JsonProperty("prizeUSD")] public decimal PrizeUSD { get; set; }

    [JsonProperty("commissionUSD")] public decimal CommissionUSD { get; set; }

    [JsonProperty("gameId")] public int GameId { get; set; }
}