// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Response;

    /// <summary>
    /// A json converter for the <see cref="ApolloServerDataMessage"/>.
    /// </summary>
    public class ApolloServerDataMessageConverter : JsonConverter<ApolloServerDataMessage>
    {
        private readonly IGraphQueryResponseWriter _responseWriter;
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloServerDataMessageConverter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="responseWriter">The response writer.</param>
        public ApolloServerDataMessageConverter(ISchema schema, IGraphQueryResponseWriter responseWriter)
        {
            _responseWriter = Validation.ThrowIfNullOrReturn(responseWriter, nameof(responseWriter));
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="ApolloServerDataMessage"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override ApolloServerDataMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(ApolloServerDataMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, ApolloServerDataMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(ApolloConstants.Messaging.MESSAGE_TYPE, ApolloMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(ApolloConstants.Messaging.MESSAGE_ID, value.Id);
            }

            writer.WritePropertyName(ApolloConstants.Messaging.MESSAGE_PAYLOAD);
            if (value.Payload == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _responseWriter.WriteAsync(
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