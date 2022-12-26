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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Messages;
    using GraphQL.AspNet.Web;

    /// <summary>
    /// A json converter for the <see cref="GraphqlWsLegacyServerErrorMessage"/> type.
    /// </summary>
    public class GraphqlWsLegacyServerErrorMessageConverter : JsonConverter<GraphqlWsLegacyServerErrorMessage>
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerErrorMessageConverter" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public GraphqlWsLegacyServerErrorMessageConverter(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public override GraphqlWsLegacyServerErrorMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GraphqlWsLegacyServerErrorMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, GraphqlWsLegacyServerErrorMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GraphqlWsLegacyConstants.Messaging.MESSAGE_TYPE, GraphqlWsLegacyMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GraphqlWsLegacyConstants.Messaging.MESSAGE_ID, value.Id);
            }

            writer.WritePropertyName(GraphqlWsLegacyConstants.Messaging.MESSAGE_PAYLOAD);
            if (value.Payload == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                var messageWriter = new SingleMessageResponseWriter(_schema);
                messageWriter.WriteSingleMessage(
                    writer,
                    value.Payload,
                    new ResponseWriterOptions()
                    {
                        ExposeExceptions = _schema.Configuration.ResponseOptions.ExposeExceptions,
                        ExposeMetrics = _schema.Configuration.ResponseOptions.ExposeMetrics,
                    });
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// A serializer that exposes the ability to write one <see cref="IGraphMessage"/> directly to a
        /// <see cref="Utf8JsonWriter"/>. This object expects the writer to be pointed at a location
        /// that an object property value can be written in place.
        /// </summary>
        private class SingleMessageResponseWriter : ResponseWriterBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SingleMessageResponseWriter" /> class.
            /// </summary>
            /// <param name="schema">The schema.</param>
            public SingleMessageResponseWriter(ISchema schema)
                : base(schema)
            {
            }

            /// <summary>
            /// Writes the single message to the provided writer.
            /// </summary>
            /// <param name="writer">The writer.</param>
            /// <param name="message">The message to serialize.</param>
            /// <param name="options">The options to use to govern the exposure of message level
            /// metadata.</param>
            public void WriteSingleMessage(Utf8JsonWriter writer, IGraphMessage message, ResponseWriterOptions options)
            {
                this.WriteMessage(writer, message, options);
            }
        }
    }
}