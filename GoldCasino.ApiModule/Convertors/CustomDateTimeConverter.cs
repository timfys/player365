using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Convertors;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
	private const string Format = "yyyy-MM-dd HH:mm:ss";

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();
		if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
		{
			return date;
		}

		// fallback to normal parsing
		return DateTime.Parse(value, CultureInfo.InvariantCulture);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(Format));
	}
}

public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
	private const string Format = "yyyy-MM-dd HH:mm:ss";

	public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var value = reader.GetString();

		// if null or empty -> return null
		if (string.IsNullOrWhiteSpace(value))
			return null;

		if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			return date;

		// fallback to normal parsing
		if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			return date;

		return null; // could not parse
	}

	public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
	{
		if (value.HasValue)
			writer.WriteStringValue(value.Value.ToString(Format));
		else
			writer.WriteNullValue();
	}
}
