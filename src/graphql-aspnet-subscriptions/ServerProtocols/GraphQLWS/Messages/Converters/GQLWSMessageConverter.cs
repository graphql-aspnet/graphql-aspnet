// *************************************************************
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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// A general converter for serializing an <see cref="GQLWSMessage" /> to json.
    /// </summary>
    internal class GQLWSMessageConverter : JsonConverter<GQLWSMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSMessageConverter"/> class.
        /// </summary>
        public GQLWSMessageConverter()
        {
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="GQLWSMessage"/>. Not Supported.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override GQLWSMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GQLWSMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GQLWSMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GQLWSConstants.Messaging.MESSAGE_TYPE, GQLWSMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GQLWSConstants.Messaging.MESSAGE_ID, value.Id);
            }

            if (value.PayloadObject != null)
            {
                writer.WritePropertyName(GQLWSConstants.Messaging.MESSAGE_PAYLOAD);
                JsonSerializer.Serialize(writer, value.PayloadObject, value.PayloadObject.GetType());
            }

            writer.WriteEndObject();
        }
    }
}