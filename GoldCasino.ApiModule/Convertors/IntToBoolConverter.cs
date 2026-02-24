using Newtonsoft.Json;
using System.Text.Json;

namespace GoldCasino.ApiModule.Convertors.SystemTextJson
{
	public partial class IntToBoolConverter : System.Text.Json.Serialization.JsonConverter<bool>
	{
		public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Number)
			{
				int intValue = reader.GetInt32();
				return intValue == 1;
			}

			if (reader.TokenType == JsonTokenType.String)
			{
				string stringValue = reader.GetString();
				if (int.TryParse(stringValue, out int intValue))
				{
					return intValue == 1;
				}
			}

			throw new System.Text.Json.JsonException($"Unexpected token {reader.TokenType} when parsing boolean.");
		}

		public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
		{
			writer.WriteNumberValue(value ? 1 : 0);
		}
	}
}

namespace GoldCasino.ApiModule.Convertors.NewtonsoftJson
{
	public class IntToBoolConverter : JsonConverter<bool>
	{
		public override bool CanRead => true;
		public override bool CanWrite => true;
		public override bool ReadJson(
			JsonReader reader,
			Type objectType,
			bool existingValue,
			bool hasExistingValue,
			Newtonsoft.Json.JsonSerializer serializer)
		{
			// If the token is null, treat it as false (or default).
			if (reader.TokenType == JsonToken.Null)
				return false;

			try
			{
				var intValue = Convert.ToInt32(reader.Value);
				return intValue == 1;
			}
			catch
			{
				return false;
			}
		}

		public override void WriteJson(JsonWriter writer, bool value, Newtonsoft.Json.JsonSerializer serializer)
		{
			writer.WriteValue(value ? 1 : 0);
		}
	}
}
