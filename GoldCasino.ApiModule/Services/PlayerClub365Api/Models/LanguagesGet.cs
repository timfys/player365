using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

public class LanguagesGetRequest
{
  public int? EntityId { get; set; }
  public string? Username { get; set; }
  public string? Password { get; set; }
  public string[]? Fields { get; set; }
  public Dictionary<string, string>? Filter { get; set; }
  public int? LimitFrom { get; set; }
  public int? LimitCount { get; set; }
}

public class Language
{
  [JsonPropertyName("languageISO")] public string LanguageIso { get; set; } = string.Empty;
  [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
  [JsonPropertyName("name_english")] public string NameEnglish { get; set; } = string.Empty;
  [JsonPropertyName("created_date")] public string CreatedDate { get; set; } = string.Empty;
  [JsonPropertyName("last_scan")] public string LastScan { get; set; } = string.Empty;
}

public class LanguagesGetResult
{
  public List<Language> Languages { get; set; } = [];
}
