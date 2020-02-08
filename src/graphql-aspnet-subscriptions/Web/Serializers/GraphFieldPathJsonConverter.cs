// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web.Serializers
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A converter to serializer and deserialize a <see cref="GraphFieldPath"/> to and from JSON.
    /// </summary>
    public class GraphFieldPathJsonConverter : JsonConverter<GraphFieldPath>
    {
        /// <summary>
        /// Reads and converts the JSON to type <see cref="GraphFieldPath" />.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override GraphFieldPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert != typeof(GraphFieldPath))
            {
                throw new JsonException($"The {typeof(GraphFieldPathJsonConverter).FriendlyName()} cannot convert {typeToConvert.FriendlyName()}.");
            }

            string fieldPath;
            if (reader.TokenType == JsonTokenType.String)
            {
                fieldPath = reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                fieldPath = null;
            }
            else
            {
                throw new JsonException($"The {typeof(GraphFieldPathJsonConverter).FriendlyName()} expects to deserialize a string containing the field path.");
            }

            return new GraphFieldPath(fieldPath);
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GraphFieldPath value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Path);
        }
    }
}