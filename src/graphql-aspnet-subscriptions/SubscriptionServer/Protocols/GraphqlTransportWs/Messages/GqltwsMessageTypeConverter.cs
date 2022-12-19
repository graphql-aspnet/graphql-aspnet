// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messages
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Handles conversion of <see cref="GqltwsMessageType"/> to and from json to handle potential casing
    /// issues with connected clients.
    /// </summary>
    internal class GqltwsMessageTypeConverter : JsonConverter<GqltwsMessageType>
    {
        /// <inheritdoc />
        public override GqltwsMessageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected {nameof(JsonTokenType.String)} but got {reader.TokenType.ToString()}");
            }

            return GqltwsMessageTypeExtensions.FromString(reader.GetString());
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, GqltwsMessageType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(GqltwsMessageTypeExtensions.Serialize(value));
        }
    }
}