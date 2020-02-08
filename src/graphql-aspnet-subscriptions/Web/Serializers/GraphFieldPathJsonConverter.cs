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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GraphFieldPath value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}