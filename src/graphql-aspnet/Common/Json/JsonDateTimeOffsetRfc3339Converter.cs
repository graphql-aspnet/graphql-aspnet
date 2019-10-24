// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Json
{
    using System;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// Converts the datetime into the RFC3339 format using the library internal
    /// extension method on <see cref="DateTimeExtensions"/>.
    /// </summary>
    public class JsonDateTimeOffsetRfc3339Converter : JsonConverter<DateTimeOffset>
    {
        /// <summary>
        /// Reads and converts the JSON to DateTimeOffset.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTimeOffset dt = default(DateTimeOffset);
            bool completed;
            if (reader.TokenType == JsonTokenType.Number)
            {
                completed = reader.TryGetInt64(out var l);
                if (completed)
                {
                    completed = DateTimeExtensions.TryParseMultiFormat(l, out var nullableDate);
                    if (completed && nullableDate.HasValue)
                        dt = nullableDate.Value;
                }
            }
            else
            {
                // internals deserializes RFC339 string with no problems most of the time
                completed = reader.TryGetDateTimeOffset(out dt);
            }

            return completed ? dt : default(DateTimeOffset);
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            try
            {
                writer.WriteStringValue(JsonEncodedText.Encode(value.ToRfc3339String(), JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
            }
            catch
            {
                writer.WriteNullValue();
            }
        }
    }
}