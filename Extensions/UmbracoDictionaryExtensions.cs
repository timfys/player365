using System.Globalization;
using Umbraco.Cms.Web.Common;

namespace SmartWinners.Extensions;

public static class UmbracoDictionaryExtensions
{
	private const string EnglishCultureName = "en";

	public static string GetDictionaryOrDefault(this UmbracoHelper? helper, string key, string fallbackValue = "")
	{
		if (helper is null)
		{
			return fallbackValue;
		}

		var localizedValue = helper.GetDictionaryValue(key);
		if (!string.IsNullOrWhiteSpace(localizedValue))
		{
			return localizedValue;
		}
		
		// if Dictionary missing then this can cause problems 
		var cal = CultureInfo.GetCultureInfo(EnglishCultureName);
		var englishValue = helper.GetDictionaryValue(key, cal);
		return string.IsNullOrWhiteSpace(englishValue) ? fallbackValue : englishValue;
	}
}
