using System.Text.Json.Serialization;
using GoldCasino.ApiModule.Mapping;

namespace GoldCasino.ApiModule.Services.SmartWinnersApi.Models;

public sealed class VouchersGet
{
	public int? LimitFrom { get; set; }
	public int? LimitCount { get; set; }
	public string[]? Fields { get; set; }
	public Dictionary<string, string>? Filter { get; set; }
}

public sealed class VouchersGetResult
{
	[JsonPropertyName("Data")]
	public List<Voucher> Data { get; init; } = [];
}

public sealed class Voucher
{
	[EntityField("voucherId")]
	[JsonPropertyName("voucherId")]
	public int VoucherId { get; set; }

	[EntityField("date_created")]
	[JsonPropertyName("date_created")]
	public string DateCreated { get; set; } = string.Empty;

	[EntityField("Name")]
	[JsonPropertyName("Name")]
	public string Name { get; set; } = string.Empty;

	[EntityField("Lines")]
	[JsonPropertyName("Lines")]
	public string Lines { get; set; } = string.Empty;

	[EntityField("discount_pct")]
	[JsonPropertyName("discount_pct")]
	public decimal DiscountPct { get; set; }

	[EntityField("discount_amount")]
	[JsonPropertyName("discount_amount")]
	public decimal DiscountAmount { get; set; }

	[EntityField("usage_max")]
	[JsonPropertyName("usage_max")]
	public int UsageMax { get; set; }

	[EntityField("usage_count")]
	[JsonPropertyName("usage_count")]
	public int UsageCount { get; set; }

	[EntityField("expiration_date")]
	[JsonPropertyName("expiration_date")]
	public string ExpirationDate { get; set; } = string.Empty;

	[EntityField("lotteries_limit")]
	[JsonPropertyName("lotteries_limit")]
	public string LotteriesLimit { get; set; } = string.Empty;

	[EntityField("serial_track")]
	[JsonPropertyName("serial_track")]
	public string SerialTrack { get; set; } = string.Empty;

	[EntityField("serial_used")]
	[JsonPropertyName("serial_used")]
	public string SerialUsed { get; set; } = string.Empty;

	[EntityField("email_messageID")]
	[JsonPropertyName("email_messageID")]
	public int EmailMessageId { get; set; }

	[EntityField("SMS_messageID")]
	[JsonPropertyName("SMS_messageID")]
	public int SmsMessageId { get; set; }
}
