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
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;

    /// <summary>
    /// A json converter for the <see cref="GQLWSServerCompleteMessage"/>.
    /// </summary>
    public class GQLWSServerCompleteMessageConverter : JsonConverter<GQLWSServerCompleteMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSServerCompleteMessageConverter" /> class.
        /// </summary>
        public GQLWSServerCompleteMessageConverter()
        {
        }

        /// <summary>
        /// Reads and converts the JSON to type <see cref="GQLWSServerDataMessage"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override GQLWSServerCompleteMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(GQLWSServerErrorMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, GQLWSServerCompleteMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(GQLWSConstants.Messaging.MESSAGE_TYPE, GQLWSMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteString(GQLWSConstants.Messaging.MESSAGE_ID, value.Id);
            }

            writer.WriteEndObject();
        }
    }
}