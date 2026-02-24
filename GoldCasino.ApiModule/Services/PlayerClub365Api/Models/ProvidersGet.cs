using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

public class ProvidersGetRequest
{
  public string LangCode { get; set; } = "en";
  public string[]? Fields { get; set; }
  public Dictionary<string, string>? Filter { get; set; }
  public int? LimitCount { get; set; }
  public int? LimitFrom { get; set; }
}

public class Provider
{
  [JsonPropertyName("provider_id")] public int Id { get; set; }
  [JsonPropertyName("provider_name")] public string? Name { get; set; }
  [JsonPropertyName("provider_logo")] public string? LogoUrl { get; set; }
  [JsonPropertyName("integratorID")] public int IntegratoreId { get; set; }
  [JsonPropertyName("parent_providerID")] public int ParentProviderId { get; set; }
  [JsonPropertyName("status")] public int Status { get; set; }
  [JsonPropertyName("provider_code")] public int? ProviderCode { get; set; }
  [JsonPropertyName("games_count")] public int GamesCount { get; set; }

  //SEO
  [JsonPropertyName("h1_title")] public string? H1Title { get; set; } = string.Empty;
  [JsonPropertyName("seo_title")] public string? Title { get; set; } = string.Empty;
  [JsonPropertyName("seo_description")] public string? Description { get; set; } = string.Empty;
  [JsonPropertyName("seo_keywords")] public string? Keywords { get; set; } = string.Empty;
  [JsonPropertyName("og_title")] public string? OgTitle { get; set; } = string.Empty;
  [JsonPropertyName("og_description")] public string? OgDescription { get; set; } = string.Empty;
  [JsonPropertyName("og_image")] public string? OgImage { get; set; } = string.Empty;
  [JsonPropertyName("twitter_title")] public string? TwitterTitle { get; set; } = string.Empty;
  [JsonPropertyName("twitter_description")] public string? TwitterDescription { get; set; } = string.Empty;
  [JsonPropertyName("twitter_image")] public string? TwitterImage { get; set; } = string.Empty;
  [JsonPropertyName("meta_robots")] public string? MetaRobots { get; set; } = string.Empty;
}

public class ProvidersGetResult
{
  public List<Provider>? Providers { get; set; }
  public int TotalCount { get; set; }
}
