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
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    /// <summary>
    /// A message sent by the server to a client acknowledging the start of a connection. This
    /// message is sent in response to CONNECTION_INIT being received.
    /// </summary>
    internal class GqltwsServerConnectionAckMessage : GqltwsMessage<GqltwsNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerConnectionAckMessage"/> class.
        /// </summary>
        public GqltwsServerConnectionAckMessage()
            : base(GqltwsMessageType.CONNECTION_ACK)
        {
        }
    }
}