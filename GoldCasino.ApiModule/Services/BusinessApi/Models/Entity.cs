using GoldCasino.ApiModule.Convertors;
using GoldCasino.ApiModule.Convertors.SystemTextJson;
using GoldCasino.ApiModule.Services.BusinessApi.Enums;
using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class Entity
{
	[JsonPropertyName("eId")] public int Id { get; set; }
	[JsonPropertyName("affiliateID")] public int AffiliateID { get; set; }
	[JsonPropertyName("EntityId")] public int EntityId { get; set; }
	[JsonPropertyName("ol_username")] public string Username { get; set; } = "";
	[JsonPropertyName("ol_password")] public string Password { get; set; } = "";
	[JsonPropertyName("FirstName")] public string FirstName { get; set; } = "";
	[JsonPropertyName("LastName")] public string LastName { get; set; } = "";
	[JsonPropertyName("Country")] public string CountryIso { get; set; } = "";


	[JsonPropertyName("mobile_verified")]
	[JsonConverter(typeof(IntToBoolConverter))] public bool MobileVerified { get; set; } = false;

	[JsonPropertyName("email_verified")]
	[JsonConverter(typeof(IntToBoolConverter))] public bool EmailVerified { get; set; } = false;

	[JsonPropertyName("Mobile")] public string PhoneNumber { get; set; } = "";
	[JsonPropertyName("Email")] public string Email { get; set; } = "";
	[JsonPropertyName("CustomField54")] public decimal BalanceUSD { get; set; } = 0;


	[JsonPropertyName("CustomField62")]
	[JsonConverter(typeof(NullableDateTimeConverter))] public DateTime? Birthday { get; set; }

	[JsonPropertyName("Address")] public string Address { get; set; } = "";
	[JsonPropertyName("City")] public string City { get; set; } = "";
	[JsonPropertyName("Zip")] public string ZipCode { get; set; } = "";
	[JsonPropertyName("State")] public string State { get; set; } = "";

	[JsonPropertyName("CustomField185")] public decimal Player1TotalWinnings { get; set; }
	[JsonPropertyName("CustomField57")] public decimal TotalWinningsUsd { get; set; }
	[JsonPropertyName("CustomField55")] public decimal TotalWithdrawAmountUsd { get; set; }
	[JsonPropertyName("CustomField104")] public decimal UserAffiliateEarnings { get; set; }
	[JsonPropertyName("CustomField131")] public int UserAffiliateReferred { get; set; }

	[JsonPropertyName("CustomField109")] public IdDocVerificationState VerificationState { get; set; }

	[JsonPropertyName("customfield85")] public string? SignUpAffiliateScript { get; set; }
	[JsonPropertyName("customfield165")] public string? AffiliateScript { get; set; }
	[JsonPropertyName("customfield46")] public string? EmailVerifyAffiliateScript { get; set; }
	[JsonPropertyName("customfield38")] public string? PaymentAffiliateScript { get; set; }
}