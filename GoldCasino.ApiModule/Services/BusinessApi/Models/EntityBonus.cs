using System.Text.Json.Serialization;
using GoldCasino.ApiModule.Convertors;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityBonus
{
  [JsonPropertyName("RecordID")] public int RecordId { get; set; }
  [JsonPropertyName("ParentRecordID")] public int ParentRecordId { get; set; }

  [JsonConverter(typeof(NullableDateTimeConverter))]
  [JsonPropertyName("CreatedDate")] public DateTime? CreatedDate { get; set; }
  [JsonPropertyName("isDeleted")] public int? IsDeleted { get; set; }

  [JsonConverter(typeof(NullableDateTimeConverter))]
  [JsonPropertyName("sync_modified_date")] public DateTime? SyncModifiedDate { get; set; }
  [JsonPropertyName("CustomField201")] public BonusType? Type { get; set; }
  [JsonPropertyName("CustomField202")] public double? Serial { get; set; }
  [JsonPropertyName("CustomField203")] public double? ValueUsd { get; set; }

  [JsonConverter(typeof(NullableDateTimeConverter))]
  [JsonPropertyName("CustomField204")] public DateTime? CreatedDateCustom { get; set; }
  [JsonPropertyName("CustomField205")] public double? Used { get; set; }

  [JsonConverter(typeof(NullableDateTimeConverter))]
  [JsonPropertyName("CustomField206")] public DateTime? ExpirationDate { get; set; }
}

public enum BonusType
{
  EmailVerification = 11,
  Ladder = 12,
  Custom = 13,
  WellocomeOffer = 14
}
