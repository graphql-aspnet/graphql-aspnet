// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Handles conversion of <see cref="GraphqlWsLegacyMessageType"/> to and from json to handle potential casing
    /// issues with connected clients.
    /// </summary>
    internal class GraphqlWsLegacyMessageTypeConverter : JsonConverter<GraphqlWsLegacyMessageType>
    {
        /// <inheritdoc />
        public override GraphqlWsLegacyMessageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected {nameof(JsonTokenType.String)} but got {reader.TokenType.ToString()}");
            }

            return GraphqlWsLegacyMessageTypeExtensions.FromString(reader.GetString());
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, GraphqlWsLegacyMessageType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(GraphqlWsLegacyMessageTypeExtensions.Serialize(value));
        }
    }
}