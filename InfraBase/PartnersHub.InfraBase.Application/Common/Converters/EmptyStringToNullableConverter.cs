using PartnersHub.InfraBase.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Application.Common.Converters;

public class EmptyStringToNullableGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Guid.Parse(value);
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value);
        else
            writer.WriteNullValue();
    }
}


public class TenderingStagesConverter : JsonConverter<TenderingStages?>
{
    public override TenderingStages? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<TenderingStages>(value, out var result))
                return result;

            throw new JsonException("Invalid TenderingStage value.");
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, TenderingStages? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.ToString()); 
        else
            writer.WriteNullValue();
    }
}



public class EmptyStringToNullableDevelopmentTypeConverter : JsonConverter<DevelopmentTypes?>
{
    public override DevelopmentTypes? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Attempt to parse the string as an enum
            if (Enum.TryParse<DevelopmentTypes>(value, out var result))
                return result;

            throw new JsonException($"Invalid DevelopmentTypes value: {value}");
        }

        if (reader.TokenType == JsonTokenType.Null)
            return null;

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, DevelopmentTypes? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.ToString()); // Serialize as string (e.g., "Greenfield")
        else
            writer.WriteNullValue();
    }
}


