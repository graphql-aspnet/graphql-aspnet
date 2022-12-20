// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.SubscriptionServer.Protocols.GraphqlTransportWs.GraphqlTransportWsData
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messages;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class GqltwsResponseMessageConverter : JsonConverter<GqltwsResponseMessage>
    {
        public override GqltwsResponseMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var message = new GqltwsResponseMessage();

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
                    case GqltwsConstants.Messaging.MESSAGE_ID:
                        message.Id = reader.GetString();
                        reader.Read();
                        break;

                    case GqltwsConstants.Messaging.MESSAGE_TYPE:
                        message.Type = GqltwsMessageTypeExtensions.FromString(reader.GetString());
                        reader.Read();
                        break;

                    case GqltwsConstants.Messaging.MESSAGE_PAYLOAD:
                        if (reader.TokenType == JsonTokenType.Null)
                            reader.Read();
                        else
                            message.Payload = reader.ReadObjectAsJsonString();

                        break;
                }
            }

            return message;
        }

        public override void Write(Utf8JsonWriter writer, GqltwsResponseMessage value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}