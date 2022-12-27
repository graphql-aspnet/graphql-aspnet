// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Messages;

    /// <summary>
    /// A json converter for the <see cref="GraphqlWsLegacyServerCompleteMessage"/>.
    /// </summary>
    internal class GraphqlWsLegacyServerCompleteMessageConverter : JsonConverter<GraphqlWsLegacyServerCompleteMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerCompleteMessageConverter" /> class.
        /// </summary>
        public GraphqlWsLegacyServerCompleteMessageConverter()
        {
        }

        /// <inheritdoc />
        public override GraphqlWsLegacyServerCompleteMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GraphqlWsLegacyServerErrorMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <inheritdoc />
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