// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messages.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messages.Common;

    /// <summary>
    /// A general converter for serializing an <see cref="GqltwsMessage" /> to json.
    /// </summary>
    internal class GqltwsMessageConverter : JsonConverter<GqltwsMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsMessageConverter"/> class.
        /// </summary>
        public GqltwsMessageConverter()
        {
        }

        /// <inheritdoc />
        public override GqltwsMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GqltwsMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, GqltwsMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GqltwsConstants.Messaging.MESSAGE_TYPE, GqltwsMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GqltwsConstants.Messaging.MESSAGE_ID, value.Id);
            }

            if (value.PayloadObject != null)
            {
                writer.WritePropertyName(GqltwsConstants.Messaging.MESSAGE_PAYLOAD);
                JsonSerializer.Serialize(writer, value.PayloadObject, value.PayloadObject.GetType());
            }

            writer.WriteEndObject();
        }
    }
}