// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Web.Serializers
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Execution.Subscriptions;

    /// <summary>
    /// A converter to serializer and deserialize a <see cref="SubscriptionEvent"/> to and from JSON.
    /// </summary>
    public class SubscriptionEventJsonConverter : JsonConverter<SubscriptionEvent>
    {
        /// <summary>
        /// Reads and converts the JSON to type <see cref="SubscriptionEvent" />.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override SubscriptionEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, SubscriptionEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}