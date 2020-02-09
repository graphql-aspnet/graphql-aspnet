﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Messages.Common
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// A general converter for serializing an <see cref="ApolloMessage"/> to json.
    /// </summary>
    public class ApolloMessageConverter : JsonConverter<ApolloMessage>
    {
        /// <summary>
        /// Reads and converts the JSON to type <see cref="ApolloMessage"/>. Not Supported.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override ApolloMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException($"{typeof(ApolloMessage).FriendlyName()} cannot be deserialized.");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, ApolloMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("type", ApolloMessageTypeExtensions.Serialize(value.Type));

            if (value.Id != null)
            {
                writer.WriteStringValue(value.Id);
            }

            if (value.PayloadObject != null)
            {
                writer.WritePropertyName("payload");
                JsonSerializer.Serialize(writer, value.PayloadObject, value.PayloadObject.GetType());
            }

            writer.WriteEndObject();
        }
    }
}