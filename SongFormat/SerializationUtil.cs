﻿using System;
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
            var normalizedString = path.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            string normalized = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            return Regex.Replace(normalized, "[^a-zA-Z0-9]", String.Empty).Trim();
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
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true
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
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true
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
