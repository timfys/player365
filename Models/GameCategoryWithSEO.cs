using System.Text.Json.Serialization;

namespace SmartWinners.Models;

public class GameCategoryWithSEO
{
	// games_categories (gc)
	[JsonPropertyName("categoryId")]
	public int CategoryID { get; set; }

	[JsonPropertyName("category_name")]
	public string CategoryName { get; set; } = string.Empty;

	[JsonPropertyName("games_count")]
	public int GamesCount { get; set; }

	[JsonPropertyName("is_dynamic")]
	public int IsDynamic { get; set; }   // still int in API, grouping uses int

	[JsonPropertyName("enabled")]
	public int Enabled { get; set; }     // API sends 1/0

	// games_categories_seo (gcs)
	[JsonPropertyName("lang_code")]
	public string LangCode { get; set; } = string.Empty;

	[JsonPropertyName("h1_title")]
	public string H1Title { get; set; } = string.Empty;

	[JsonPropertyName("seo_title")]
	public string Title { get; set; } = string.Empty;

	[JsonPropertyName("seo_description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("seo_keywords")]
	public string Keywords { get; set; } = string.Empty;

	[JsonPropertyName("seo_slug")]
	public string Slug { get; set; } = string.Empty;

	[JsonPropertyName("og_title")]
	public string OgTitle { get; set; } = string.Empty;

	[JsonPropertyName("og_description")]
	public string OgDescription { get; set; } = string.Empty;

	[JsonPropertyName("og_image")]
	public string OgImage { get; set; } = string.Empty;

	[JsonPropertyName("twitter_title")]
	public string TwitterTitle { get; set; } = string.Empty;

	[JsonPropertyName("twitter_description")]
	public string TwitterDescription { get; set; } = string.Empty;

	[JsonPropertyName("twitter_image")]
	public string TwitterImage { get; set; } = string.Empty;

	[JsonPropertyName("meta_robots")]
	public string MetaRobots { get; set; } = string.Empty;

	// helper property for Enabled (bool shortcut)
	[JsonIgnore]
	public bool IsActive => Enabled == 1;
}