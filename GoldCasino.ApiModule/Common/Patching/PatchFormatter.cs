using System.Globalization;

namespace GoldCasino.ApiModule.Common.Patching;
public static class PatchFormatter
{
	/// <summary>
	/// Default SOAP-ish formatter:
	/// - null → "" (clear)
	/// - bool → "1"/"0"
	/// - DateTime/Offset → "yyyy-MM-dd HH:mm:ss" (UTC for offsets)
	/// - numerics → invariant
	/// - others → ToString()
	/// </summary>
	public static string Soap(object? value)
	{
		if (value is null) return string.Empty;

		if (value is bool b) return b ? "1" : "0";

		if (value is DateTime dt)
			return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

		if (value is DateTimeOffset dto)
			return dto.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

		// If you need enums as numbers, change this to Convert.ToInt32(value) or add an option
		if (value is IFormattable f)
			return f.ToString(null, CultureInfo.InvariantCulture);

		return value.ToString() ?? string.Empty;
	}
}