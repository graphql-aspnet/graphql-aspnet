// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;

    /// <summary>
    /// A general converter for serializing an <see cref="GraphqlWsLegacyMessage" /> to json.
    /// </summary>
    public class GraphqlWsLegacyMessageConverter : JsonConverter<GraphqlWsLegacyMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyMessageConverter"/> class.
        /// </summary>
        public GraphqlWsLegacyMessageConverter()
        {
        }

        /// <inheritdoc />
        public override GraphqlWsLegacyMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GraphqlWsLegacyMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, GraphqlWsLegacyMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GraphqlWsLegacyConstants.Messaging.MESSAGE_TYPE, GraphqlWsLegacyMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GraphqlWsLegacyConstants.Messaging.MESSAGE_ID, value.Id);
            }

            if (value.PayloadObject != null)
            {
                writer.WritePropertyName(GraphqlWsLegacyConstants.Messaging.MESSAGE_PAYLOAD);
                JsonSerializer.Serialize(writer, value.PayloadObject, value.PayloadObject.GetType());
            }

            writer.WriteEndObject();
        }
    }
}