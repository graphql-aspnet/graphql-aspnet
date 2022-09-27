// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.ServerProtocols.GraphqlWsLegacy.GraphqlWsLegacyData
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class GraphqlWsLegacyResponseMessageConverter : JsonConverter<GraphqlWsLegacyResponseMessage>
    {
        public override GraphqlWsLegacyResponseMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var message = new GraphqlWsLegacyResponseMessage();

            if (reader.TokenType == JsonTokenType.StartObject)
                reader.Read();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new InvalidOperationException("expected prop name");

                var propName = reader.GetString();
                reader.Read();

                switch (propName.ToLower())
                {
                    case GraphqlWsLegacyConstants.Messaging.MESSAGE_ID:
                        message.Id = reader.GetString();
                        reader.Read();
                        break;

                    case GraphqlWsLegacyConstants.Messaging.MESSAGE_TYPE:
                        message.Type = GraphqlWsLegacyMessageTypeExtensions.FromString(reader.GetString());
                        reader.Read();
                        break;

                    case GraphqlWsLegacyConstants.Messaging.MESSAGE_PAYLOAD:
                        if (reader.TokenType == JsonTokenType.Null)
                            reader.Read();
                        else
                            message.Payload = reader.ReadObjectAsJsonString();

                        break;
                }
            }

            return message;
        }

        public override void Write(Utf8JsonWriter writer, GraphqlWsLegacyResponseMessage value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}