namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityAddResult
{
    public string EntityId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public bool CustomerExists { get; set; }
    public string? AffiliateResultCode { get; set; }
}
