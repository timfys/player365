using GoldCasino.ApiModule.Mapping;
using System.Collections.Generic;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

public class GamesUpdateRequest
{
  public int GameId { get; set; }
  public Dictionary<string, string>? Fields { get; set; }

  [EntityField("game_name")] public string? Name { get; set; }
  [EntityField("game_image")] public string? ImageUrl { get; set; }
  [EntityField("categoryID")] public int? CategoryId { get; set; }
  [EntityField("integratorID")] public int? IntegratorId { get; set; }
  [EntityField("provider_id")] public int? StudioId { get; set; }
  [EntityField("game_code")] public string? GameCode { get; set; }
  [EntityField("launch_enable")] public int? Enabled { get; set; }
  [EntityField("video_recorded")] public string? VideoRecorded { get; set; }
  [EntityField("Hall_Balance")] public decimal? HallBalance { get; set; }
  [EntityField("game_description")] public string? GameDescription { get; set; }
  [EntityField("h1_title")] public string? H1Title { get; set; }
  [EntityField("rules_layout")] public string? RulesLayout { get; set; }
  [EntityField("how_to_play")] public string? HowToPlay { get; set; }
  [EntityField("main_features")] public string? MainFeatures { get; set; }
  [EntityField("tips_for_new_players")] public string? TipsForNewPlayers { get; set; }
  [EntityField("device_compatibility")] public string? DeviceCompatibility { get; set; }
  [EntityField("why_choose_playerclub365")] public string? WhyChoosePlayerClub365 { get; set; }
  [EntityField("responsible_play")] public string? ResponsiblePlay { get; set; }
  [EntityField("closing_line")] public string? ClosingLine { get; set; }
  [EntityField("image_alt_text")] public string? ImageAltText { get; set; }
  [EntityField("seo_title")] public string? SeoTitle { get; set; }
  [EntityField("seo_description")] public string? SeoDescription { get; set; }
  [EntityField("seo_keywords")] public string? SeoKeywords { get; set; }
  [EntityField("seo_slug")] public string? SeoSlug { get; set; }
  [EntityField("og_title")] public string? OgTitle { get; set; }
  [EntityField("og_description")] public string? OgDescription { get; set; }
  [EntityField("og_image")] public string? OgImage { get; set; }
  [EntityField("twitter_title")] public string? TwitterTitle { get; set; }
  [EntityField("twitter_description")] public string? TwitterDescription { get; set; }
  [EntityField("twitter_image")] public string? TwitterImage { get; set; }
  [EntityField("meta_robots")] public string? MetaRobots { get; set; }
  [EntityField("canonical_url")] public string? CanonicalUrl { get; set; }
}

public class GamesUpdateResult
{
  public List<Game>? Games { get; set; }
}
