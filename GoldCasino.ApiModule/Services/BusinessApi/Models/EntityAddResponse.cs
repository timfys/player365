using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityAddResponse : ApiResponse
{
    public string EntityId { get; set; } = string.Empty;

    [JsonPropertyName("ol_username")]
    public string? Username { get; set; }

    public string? AffiliateResultCode { get; set; }

    public const int ResultCodeCustomerExists = -5674;
    public bool IsCustomerExists() => ResultCode == ResultCodeCustomerExists;
}
