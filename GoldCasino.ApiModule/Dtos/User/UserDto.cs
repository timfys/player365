using GoldCasino.ApiModule.Mapping;
using GoldCasino.ApiModule.Services.BusinessApi.Enums;

namespace GoldCasino.ApiModule.Dtos.User;

public class UserDto
{
	[EntityField("EntityId")] public int Id { get; set; }
	[EntityField("affiliateID")] public int AffiliateID { get; set; }
	[EntityField("ol_username")] public string Username { get; set; } = "";
	[EntityField("FirstName")] public string FirstName { get; set; } = "";
	[EntityField("LastName")] public string LastName { get; set; } = "";
	[EntityField("Country")] public string CountryIso { get; set; } = "";

	[EntityField("mobile_verified")] public bool MobileVerified { get; set; } = false;
	[EntityField("email_verified")] public bool EmailVerified { get; set; } = false;

	[EntityField("Mobile")] public string PhoneNumber { get; set; } = "";
	[EntityField("Email")] public string Email { get; set; } = "";

	[EntityField("CustomField62")] public DateTime? Birthday { get; set; }


	[EntityField("Address")] public string Address { get; set; } = "";
	[EntityField("City")] public string City { get; set; } = "";
	[EntityField("Zip")] public string ZipCode { get; set; } = "";
	[EntityField("State")] public string State { get; set; } = "";
}

public class UserAccessDto
{
	[EntityField("EntityId")] public int Id { get; set; }
	[EntityField("ol_username")] public string Username { get; set; } = "";
	[EntityField("ol_password")] public string PasswordHash { get; set; } = "";
}

public class UserBalanceDto
{
	[EntityField("CustomField54")]
	public decimal BalanceUSD { get; set; } = 0;
}

public class LegacyUserBalancesDto
{
	[EntityField("CustomField185")] public decimal Player1TotalWinnings { get; set; }
	[EntityField("CustomField57")] public decimal TotalWinningsUsd { get; set; }
	[EntityField("CustomField55")] public decimal TotalWithdrawAmountUsd { get; set; }
	[EntityField("CustomField104")] public decimal UserAffiliateEarnings { get; set; }
	[EntityField("CustomField131")] public int UserAffiliateReferred { get; set; }
}

public class UserVerificationStateDto
{
	[EntityField("CustomField109")]
	public IdDocVerificationState VerificationState { get; set; }
}

public class UserAffiliateDto
{
	[EntityField("affiliateID")] public int AffiliateId { get; set; }
}