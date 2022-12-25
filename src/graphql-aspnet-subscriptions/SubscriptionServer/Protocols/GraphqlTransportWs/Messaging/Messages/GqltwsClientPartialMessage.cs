// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages
{
    using System.Diagnostics;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    /// <summary>
    /// A partially deserialized operation message recieved from the client. Converts the actual paylaod
    /// into a collection of key/value variables for later parsing. Used as an intermediary to prevent
    /// double deserialization of a message via json deserializer.
    /// </summary>
    [DebuggerDisplay("Message Type: {Type}")]
    public class GqltwsClientPartialMessage : GqltwsMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientPartialMessage"/> class.
        /// </summary>
        public GqltwsClientPartialMessage()
            : base(GqltwsMessageType.UNKNOWN)
        {
        }

        /// <summary>
        /// Converts this instance into its final, payload focused message.
        /// </summary>
        /// <returns>A fully qualified message.</returns>
        public GqltwsMessage Convert()
        {
            switch (this.Type)
            {
                case GqltwsMessageType.CONNECTION_INIT:
                    return new GqltwsClientConnectionInitMessage()
                    {
                        Payload = null, // TODO: connection may have params, need to handle it
                    };

                case GqltwsMessageType.SUBSCRIBE:
                    return new GqltwsClientSubscribeMessage()
                    {
                        Id = this.Id,
                        Payload = this.Payload,
                    };

                case GqltwsMessageType.COMPLETE:
                    return new GqltwsSubscriptionCompleteMessage(this.Id)
                    {
                        Payload = null, // stop message has no expected payload
                    };

                case GqltwsMessageType.PING:
                    return new GqltwsPingMessage();

                case GqltwsMessageType.PONG:
                    return new GqltwsPongMessage();

                default:
                    return new GqltwsUnknownMessage()
                    {
                        Id = this.Id,
                    };
            }
        }
    }
}