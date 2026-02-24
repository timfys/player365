using System.Text.Json.Serialization;
namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public sealed class EntityBonusesGet
{
  public int? LimitFrom { get; set; }
  public int? LimitCount { get; set; }
  public string[]? Fields { get; set; }
  public Dictionary<string, string>? Filter { get; set; }
}

public sealed class EntityBonusesGetResult
{
  [JsonPropertyName("Data")]
  public List<EntityBonus> Data { get; init; } = [];
}
