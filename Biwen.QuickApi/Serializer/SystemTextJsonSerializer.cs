// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:54:02 SystemTextJsonSerializer.cs

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Biwen.QuickApi.Serializer;

public class SystemTextJsonSerializer : ITextSerializer
{
    private readonly JsonSerializerOptions _serializeOptions;
    private readonly JsonSerializerOptions _deserializeOptions;

    public SystemTextJsonSerializer(JsonSerializerOptions? serializeOptions = null, JsonSerializerOptions? deserializeOptions = null)
    {
        if (serializeOptions != null)
        {
            _serializeOptions = serializeOptions;
        }
        else
        {
            _serializeOptions = new JsonSerializerOptions();
        }

        if (deserializeOptions != null)
        {
            _deserializeOptions = deserializeOptions;
        }
        else
        {
            _deserializeOptions = new JsonSerializerOptions();
            _deserializeOptions.Converters.Add(new ObjectToInferredTypesConverter());
        }
    }

    public void Serialize(object data, Stream outputStream)
    {
        var writer = new Utf8JsonWriter(outputStream);
        JsonSerializer.Serialize(writer, data, data.GetType(), _serializeOptions);
        writer.Flush();
    }

    public object Deserialize(Stream inputStream, Type objectType)
    {
        using var reader = new StreamReader(inputStream);
        return JsonSerializer.Deserialize(reader.ReadToEnd(), objectType, _deserializeOptions)!;
    }
}

public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        if (reader.TokenType == JsonTokenType.Number)
            return reader.TryGetInt64(out long number) ? number : (object)reader.GetDouble();

        if (reader.TokenType == JsonTokenType.String)
            return reader.TryGetDateTime(out var datetime) ? datetime : reader.GetString();

        using var document = JsonDocument.ParseValue(ref reader);

        return document.RootElement.Clone();
    }

    public override void Write(Utf8JsonWriter writer, object objectToWrite, JsonSerializerOptions options)
    {
        throw new InvalidOperationException();
    }
}
