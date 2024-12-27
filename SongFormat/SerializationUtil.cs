using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SongFormat
{
    public static class SerializationUtil
    {
        public static string GetSafeFilename(string path)
        {
            return Regex.Replace(path, "[^a-zA-Z0-9]", String.Empty).Trim();
        }

        public static JsonSerializerOptions IndentedSerializerOptions { get; private set; } = new JsonSerializerOptions()
        {
            Converters = {
               new JsonStringEnumConverter(),
               new JsonConverterFloatRound()
            },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DefaultValueModifier }
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true,
        };

        public static JsonSerializerOptions CondensedSerializerOptions { get; private set; } = new JsonSerializerOptions()
        {
            Converters = {
               new JsonStringEnumConverter(),
               new JsonConverterFloatRound()
            },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DefaultValueModifier }
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        private class JsonConverterFloatRound : JsonConverter<float>
        {
            public override float Read(ref Utf8JsonReader reader,
                Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetSingle();
            }

            public override void Write(Utf8JsonWriter writer, float value,
                JsonSerializerOptions options)
            {
                writer.WriteRawValue(value.ToString("0.###",
                    CultureInfo.InvariantCulture));
            }
        }

        private static void DefaultValueModifier(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Kind != JsonTypeInfoKind.Object)
                return;

            foreach (var property in typeInfo.Properties)
                if (property.PropertyType == typeof(int))
                {
                    property.ShouldSerialize = (_, val) => ((int)val != -1);
                }
        }
    }
}
