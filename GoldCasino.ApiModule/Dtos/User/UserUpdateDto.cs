using GoldCasino.ApiModule.Mapping;
using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Dtos.User;

public class UserUpdateDto
{
	[EntityField("ol_username")] public string? Username { get; set; }
	[EntityField("FirstName")] public string? FirstName { get; set; }
	[EntityField("LastName")] public string? LastName { get; set; }
	[EntityField("Country")] public string? CountryIso { get; set; }
	[EntityField("Mobile")] public string? PhoneNumber { get; set; }
	[EntityField("Email")] public string? Email { get; set; }

	[EntityField("CustomField62")] public DateTime? Birthday { get; set; }

	[EntityField("Address")] public string? Address { get; set; }
	[EntityField("City")] public string? City { get; set; }
	[EntityField("Zip")] public string? ZipCode { get; set; }
	[EntityField("State")] public string? State { get; set; }

	[EntityField("ol_password")] public string? Password { get; set; }
}

public sealed class UserCategoryIdDto
{
	[EntityField("CategoryID")] public int UserCategoryId { get; set; }
}

public sealed class UserSignUpDataDto
{
	[EntityField("categoryID")] public string? UserCategoryId { get; set; } 
	[EntityField("CustomField3")] public string? SignUpDate { get; set; }
	[EntityField("CustomField4")] public string? LastLogin { get; set; }
	[EntityField("CustomField9")] public string? SignUpIP { get; set; }
	[EntityField("CustomField10")] public string? LoginIp { get; set; }
	[EntityField("CustomField67")] public string? Language { get; set; }
	[EntityField("CustomField68")] public string? SignUpDomain { get; set; }
	[EntityField("CustomField71")] public string? Referer { get; set; }
}

public sealed class EntityImagesDto
{
	[JsonPropertyName("KycImageFront")] public string? KycImageFront { get; set; }
	[JsonPropertyName("KycImageBack")] public string? KycImageBack { get; set; }
}