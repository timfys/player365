using GoldCasino.ApiModule.Mapping;

namespace GoldCasino.ApiModule.Dtos.Transaction;

public class GameTransactionDto
{
  public int GameTransactionID { get; set; }
  [EntityField("gt.gameID")] public int GameID { get; set; }
  [EntityField("gt.entityID")] public int EntityId { get; set; }
  [EntityField("transaction_Type")] public int TransactionType { get; set; }
  [EntityField("amountUSD")] public decimal AmountUSD { get; set; }
  [EntityField("gt.sync_modified_date")] public string SyncModifiedDate { get; set; }
  [EntityField("pg.game_name")] public string Name { get; set; }
}
