using GoldCasino.ApiModule.Mapping;

namespace GoldCasino.ApiModule.Dtos.Providers;

public class ProviderMinDto
{
  public int Id { get; set; }
  [EntityField("ps.provider_name")] public string? Name { get; set; }
  [EntityField("provider_logo")] public string? LogoUrl { get; set; }
}

public class ProviderSimpleDto
{
  public int Id { get; set; }
  [EntityField("ps.provider_name")] public string? Name { get; set; }
  [EntityField("provider_logo")] public string? LogoUrl { get; set; }
  [EntityField("integratorID")] public int IntegratoreId { get; set; }
  [EntityField("parent_providerID")] public int ParentProviderId { get; set; }
  [EntityField("status")] public int Status { get; set; }
  [EntityField("games_count")] public int GamesCount { get; set; }
}

public class ProviderDetailedDto
{
  public int Id { get; set; }

  [EntityField("provider_logo")] public string? LogoUrl { get; set; }
  [EntityField("integratorID")] public int IntegratoreId { get; set; }
  [EntityField("parent_providerID")] public int ParentProviderId { get; set; }
  [EntityField("status")] public int Status { get; set; }
  [EntityField("provider_code")] public int? ProviderCode { get; set; }
  [EntityField("games_count")] public int GamesCount { get; set; }

  //SEO
  [EntityField("ps.provider_name")] public string? Name { get; set; }
  [EntityField("ps.h1_title")] public string? H1Title { get; set; } = string.Empty;
  [EntityField("ps.seo_title")] public string? Title { get; set; } = string.Empty;
  [EntityField("ps.seo_description")] public string? Description { get; set; } = string.Empty;
  [EntityField("ps.seo_keywords")] public string? Keywords { get; set; } = string.Empty;
  [EntityField("ps.og_title")] public string? OgTitle { get; set; } = string.Empty;
  [EntityField("ps.og_description")] public string? OgDescription { get; set; } = string.Empty;
  [EntityField("ps.twitter_title")] public string? TwitterTitle { get; set; } = string.Empty;
  [EntityField("ps.twitter_description")] public string? TwitterDescription { get; set; } = string.Empty;
  [EntityField("ps.meta_robots")] public string? MetaRobots { get; set; } = string.Empty;
}


