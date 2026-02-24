using PhoneNumbers;

namespace GoldCasino.ApiModule.Helpers;

public static class PhoneHelper
{
	private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

	/// <summary>
	/// Returns the country calling code (e.g. "1" for USA/Canada) from a given phone number.
	/// </summary>
	/// <param name="phoneNumber">The raw phone number string (with or without +).</param>
	/// <param name="defaultRegion">
	/// Optional: Default ISO country code (e.g., "US") for parsing if number does not start with '+'.
	/// </param>
	/// <returns>String country code, or null if parsing fails.</returns>
	public static string? GetCountryCode(string phoneNumber, string defaultRegion = "US")
	{
		if (string.IsNullOrWhiteSpace(phoneNumber))
			return null;

		try
		{
			var parsed = _phoneUtil.Parse(phoneNumber, defaultRegion);
			return parsed.CountryCode.ToString();
		}
		catch (NumberParseException)
		{
			return null;
		}
	}

	public static PhoneNumber? Parse(string phoneNumber, string defaultRegion = "US")
	{
		if (string.IsNullOrWhiteSpace(phoneNumber))
			return null;

		try
		{
			var parsed = _phoneUtil.Parse(phoneNumber, defaultRegion);
			return parsed;
		}
		catch (NumberParseException)
		{
			return null;
		}
	}


}