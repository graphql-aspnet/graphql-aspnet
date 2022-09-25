﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;

    /// <summary>
    /// A json converter for the <see cref="GQLWSServerErrorMessage"/> type.
    /// </summary>
    internal class GQLWSServerErrorMessageConverter : JsonConverter<GQLWSServerErrorMessage>
    {
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSServerErrorMessageConverter" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public GQLWSServerErrorMessageConverter(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="GQLWSServerNextDataMessage"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override GQLWSServerErrorMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GQLWSServerErrorMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GQLWSServerErrorMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GQLWSConstants.Messaging.MESSAGE_TYPE, GQLWSMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GQLWSConstants.Messaging.MESSAGE_ID, value.Id);
            }

            writer.WritePropertyName(GQLWSConstants.Messaging.MESSAGE_PAYLOAD);
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
                    new GraphQLResponseOptions()
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
        private class SingleMessageResponseWriter : BaseResponseWriter
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
            public void WriteSingleMessage(Utf8JsonWriter writer, IGraphMessage message, GraphQLResponseOptions options)
            {
                this.WriteMessage(writer, message, options);
            }
        }
    }
}