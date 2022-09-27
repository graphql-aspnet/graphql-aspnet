// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.ServerMessages;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// A json converter for the <see cref="GraphqlWsLegacyServerCompleteMessage"/>.
    /// </summary>
    public class GraphqlWsLegacyServerCompleteMessageConverter : JsonConverter<GraphqlWsLegacyServerCompleteMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerCompleteMessageConverter" /> class.
        /// </summary>
        public GraphqlWsLegacyServerCompleteMessageConverter()
        {
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="GraphqlWsLegacyServerDataMessage"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override GraphqlWsLegacyServerCompleteMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GraphqlWsLegacyServerErrorMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GraphqlWsLegacyServerCompleteMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GraphqlWsLegacyConstants.Messaging.MESSAGE_TYPE, GraphqlWsLegacyMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GraphqlWsLegacyConstants.Messaging.MESSAGE_ID, value.Id);
            }

            writer.WriteEndObject();
        }
    }
}