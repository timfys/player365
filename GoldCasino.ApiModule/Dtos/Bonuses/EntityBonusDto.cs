using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.BusinessApi.Models;

namespace GoldCasino.ApiModule.Dtos.Bonuses;

public class EntityBonusDto
{
  [EntityField("RecordID")] public int RecordId { get; set; }
  [EntityField("ParentRecordID")] public int ParentRecordId { get; set; }
  [EntityField("CreatedDate")] public DateTime? CreatedDate { get; set; }
  [EntityField("isDeleted")] public int? IsDeleted { get; set; }
  [EntityField("sync_modified_date")] public DateTime? SyncModifiedDate { get; set; }
  [EntityField("CustomField201")] public BonusType? Type { get; set; }
  [EntityField("CustomField202")] public double? Serial { get; set; }
  [EntityField("CustomField203")] public double? ValueUsd { get; set; }
  [EntityField("CustomField204")] public DateTime? CreatedDateCustom { get; set; }
  [EntityField("CustomField205")] public double? Used { get; set; }
  [EntityField("CustomField206")] public DateTime? ExpirationDate { get; set; }
}