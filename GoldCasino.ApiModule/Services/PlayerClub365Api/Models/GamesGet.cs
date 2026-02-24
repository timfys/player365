using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

public class GamesGetRequest
{
	public int? EntityId { get; set; }
	public string? Username { get; set; }
	public string? Password { get; set; }
  public string LangCode { get; set; } = "en";
  public string[]? Fields { get; set; }
  public Dictionary<string, string>? Filter { get; set; }
  public int? LimitCount { get; set; }
  public int? LimitFrom { get; set; }
}

public class Game
{
	[JsonPropertyName("gameId")] public int Id { get; set; }
	[JsonPropertyName("game_image")] public string ImageUrl { get; set; }
	[JsonPropertyName("game_name")] public string Name { get; set; }
	[JsonPropertyName("provider_id")] public int StudioId { get; set; }
	[JsonPropertyName("integratorID")] public int IntegratoreId { get; set; }
	[JsonPropertyName("game_code")] public string GameCode { get; set; }
	
	[JsonPropertyName("Hall_Balance")] public decimal HallBalance { get; set; }
  
	//
	[JsonPropertyName("categoryId")] public int CategoryID { get; set; }
	[JsonPropertyName("game_description")] public string GameDescription { get; set; } = string.Empty;
	[JsonPropertyName("launch_enable")] public int Enabled { get; set; }     // API sends 1/0
																																					 // games_categories_seo (gcs)
	[JsonPropertyName("video_recorded")] public string VideoRecorded { get; set; } = string.Empty;

	[JsonPropertyName("rules_layout")] public string? RulesLayout { get; set; }
  [JsonPropertyName("how_to_play")] public string? HowToPlay { get; set; }
  [JsonPropertyName("main_features")] public string? MainFeatures { get; set; }
  [JsonPropertyName("tips_for_new_players")] public string? TipsForNewPlayers { get; set; }
  [JsonPropertyName("device_compatibility")] public string? DeviceCompatibility { get; set; }
  [JsonPropertyName("why_choose_playerclub365")] public string? WhyChoosePlayerClub365 { get; set; }
  [JsonPropertyName("responsible_play")] public string? ResponsiblePlay { get; set; }
  [JsonPropertyName("closing_line")] public string? ClosingLine { get; set; }
  [JsonPropertyName("image_alt_text")] public string? ImageAltText { get; set; }

	[JsonPropertyName("lang_code")] public string LangCode { get; set; } = string.Empty;
	[JsonPropertyName("h1_title")] public string H1Title { get; set; } = string.Empty;
	[JsonPropertyName("seo_title")] public string Title { get; set; } = string.Empty;
	[JsonPropertyName("seo_description")] public string Description { get; set; } = string.Empty;
	[JsonPropertyName("seo_keywords")] public string Keywords { get; set; } = string.Empty;
	[JsonPropertyName("seo_slug")] public string Slug { get; set; } = string.Empty;
	[JsonPropertyName("og_title")] public string OgTitle { get; set; } = string.Empty;
	[JsonPropertyName("og_description")] public string OgDescription { get; set; } = string.Empty;
	[JsonPropertyName("og_image")] public string OgImage { get; set; } = string.Empty;
	[JsonPropertyName("twitter_title")] public string TwitterTitle { get; set; } = string.Empty;
	[JsonPropertyName("twitter_description")] public string TwitterDescription { get; set; } = string.Empty;
	[JsonPropertyName("twitter_image")] public string TwitterImage { get; set; } = string.Empty;
	[JsonPropertyName("meta_robots")] public string MetaRobots { get; set; } = string.Empty;
	[JsonPropertyName("canonical_url")] public string Canonical { get; set; } = string.Empty;
}

public class GamesGetResult
{
  public List<Game>? Games { get; set; }
  public int TotalCount { get; set; }
}

