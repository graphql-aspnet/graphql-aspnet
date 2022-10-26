// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages;

    /// <summary>
    /// A json converter for the <see cref="GqltwsServerNextDataMessage"/>.
    /// </summary>
    internal class GqltwsServerDataMessageConverter : JsonConverter<GqltwsServerNextDataMessage>
    {
        private readonly IGraphQueryResponseWriter _responseWriter;
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerDataMessageConverter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="responseWriter">The response writer.</param>
        public GqltwsServerDataMessageConverter(ISchema schema, IGraphQueryResponseWriter responseWriter)
        {
            _responseWriter = Validation.ThrowIfNullOrReturn(responseWriter, nameof(responseWriter));
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public override GqltwsServerNextDataMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GqltwsServerNextDataMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, GqltwsServerNextDataMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GqltwsConstants.Messaging.MESSAGE_TYPE, GqltwsMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GqltwsConstants.Messaging.MESSAGE_ID, value.Id);
            }

            writer.WritePropertyName(GqltwsConstants.Messaging.MESSAGE_PAYLOAD);
            if (value.Payload == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _responseWriter.Write(
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
    }
}