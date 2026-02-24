using GoldCasino.ApiModule.Mapping;

namespace GoldCasino.ApiModule.Dtos.Languages;

public class LanguageDto
{
  [EntityField("languageISO")] public string LanguageIso { get; set; } = string.Empty;
  [EntityField("name")] public string Name { get; set; } = string.Empty;
  [EntityField("name_english")] public string NameEnglish { get; set; } = string.Empty;
  [EntityField("created_date")] public string CreatedDate { get; set; } = string.Empty;
  [EntityField("last_scan")] public string LastScan { get; set; } = string.Empty;
}
