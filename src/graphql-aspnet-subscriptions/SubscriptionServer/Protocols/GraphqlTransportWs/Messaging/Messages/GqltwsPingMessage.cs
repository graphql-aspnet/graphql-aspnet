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
    /// A representation of the PING message sent by a client or server to the other
    /// end of the connection.
    /// </summary>
    internal class GqltwsPingMessage : GqltwsMessage<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsPingMessage"/> class.
        /// </summary>
        public GqltwsPingMessage()
            : base(GqltwsMessageType.PING)
        {
        }
    }
}