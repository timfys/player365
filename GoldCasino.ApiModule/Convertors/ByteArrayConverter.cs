using System.Text.Json;
using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Convertors;

public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var bytes = new List<byte>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return bytes.ToArray();

                if (reader.TokenType == JsonTokenType.Number)
                    bytes.Add(reader.GetByte());
            }
        }

        throw new JsonException("Invalid byte array format");
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var b in value)
            writer.WriteNumberValue(b);
        writer.WriteEndArray();
    }
}