using System.Text.Json;
using System.Text.Json.Serialization;

namespace PartnersHub.InfraBase.Apis.Common;

public class NullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            
            if (string.IsNullOrWhiteSpace(stringValue) || 
                stringValue.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                stringValue.Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (Guid.TryParse(stringValue, out var guid))
            {
                return guid;
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to Guid.");
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

public class GuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            
            if (string.IsNullOrWhiteSpace(stringValue) || 
                stringValue.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                stringValue.Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.Empty;
            }

            if (Guid.TryParse(stringValue, out var guid))
            {
                return guid;
            }

            throw new JsonException($"Unable to convert \"{stringValue}\" to Guid.");
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
