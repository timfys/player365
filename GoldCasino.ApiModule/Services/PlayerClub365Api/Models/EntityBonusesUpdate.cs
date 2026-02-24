using GoldCasino.ApiModule.Dtos.Bonuses;
using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.BusinessApi.Models;

namespace GoldCasino.ApiModule.Services.PlayerClub365Api.Models;

public class EntityBonusesUpdate
{
  public int RecordId { get; set; }
  public int EntityId { get; set; }
  public BonusType BonusType { get; set; }
  public int Serial { get; set; }
  public decimal Value { get; set; }

  [EntityField("CreatedDate")] public DateTime? CreatedDate { get; set; }
  [EntityField("isDeleted")] public bool? IsDeleted { get; set; }
  [EntityField("CustomField204")] public DateTime? CreatedDateCustom { get; set; }
  [EntityField("CustomField205")] public decimal? Used { get; set; }
  [EntityField("CustomField206")] public DateTime? ExpirationDate { get; set; }
}
