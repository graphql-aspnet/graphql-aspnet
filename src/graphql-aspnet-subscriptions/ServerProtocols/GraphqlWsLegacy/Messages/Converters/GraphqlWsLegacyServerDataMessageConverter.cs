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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Response;

    /// <summary>
    /// A json converter for the <see cref="GraphqlWsLegacyServerDataMessage"/>.
    /// </summary>
    public class GraphqlWsLegacyServerDataMessageConverter : JsonConverter<GraphqlWsLegacyServerDataMessage>
    {
        private readonly IGraphQueryResponseWriter _responseWriter;
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerDataMessageConverter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="responseWriter">The response writer.</param>
        public GraphqlWsLegacyServerDataMessageConverter(ISchema schema, IGraphQueryResponseWriter responseWriter)
        {
            _responseWriter = Validation.ThrowIfNullOrReturn(responseWriter, nameof(responseWriter));
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="GraphqlWsLegacyServerDataMessage"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override GraphqlWsLegacyServerDataMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GraphqlWsLegacyServerDataMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GraphqlWsLegacyServerDataMessage value, JsonSerializerOptions options)
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