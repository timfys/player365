using System;
using System.Text.RegularExpressions;

namespace SmartWinners.Controllers;

internal static class GamesQueryHelper
{
	private const string DesktopInClause = "in ('Desktop','Desktop and Mobile')";
	private const string MobileInClause = "in ('Mobile','Desktop and Mobile')";
	private const string AllDevicesInClause = "in ('Desktop','Mobile','Desktop and Mobile')";
	private const string DesktopAndMobileOnlyInClause = "in ('Desktop and Mobile')";

	public static string CleanQuery(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return string.Empty;

		var cleaned = Regex.Replace(value, @"\p{C}+", " ").Trim();
		return cleaned.Length > 100 ? cleaned[..100] : cleaned;
	}

	public static string EscapeForSqlLike(string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		var escaped = value.Replace("\\", "\\\\");
		escaped = escaped.Replace("%", "\\%").Replace("_", "\\_");
		escaped = escaped.Replace("'", "''");
		return escaped;
	}

	public static string? DeviceFilterFromQueryOrUa(string deviceParam, string userAgent)
	{
		if (!string.IsNullOrWhiteSpace(deviceParam))
		{
			var dv = deviceParam.Trim().ToLowerInvariant();
			var simplified = dv.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty);
			switch (simplified)
			{
				case "desktop" or "0" or "1":
					return DesktopInClause;
				case "mobile" or "2":
					return MobileInClause;
				case "desktopandmobile":
					return DesktopAndMobileOnlyInClause;
				case "all" or "3":
					return AllDevicesInClause;
			}
		}

		var ua = userAgent ?? string.Empty;
		var isMobile = ua.Contains("Mobi", StringComparison.OrdinalIgnoreCase)
					|| ua.Contains("Android", StringComparison.OrdinalIgnoreCase)
					|| ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
					|| ua.Contains("iPad", StringComparison.OrdinalIgnoreCase)
					|| ua.Contains("iPod", StringComparison.OrdinalIgnoreCase);

		return isMobile ? MobileInClause : DesktopInClause;
	}
}
