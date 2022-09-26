// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages
{
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;

    /// <summary>
    /// A representation of the PONG message sent by a client or server to the other
    /// end of the connection after receiving a PING message.
    /// </summary>
    public class GqltwsPongMessage : GqltwsMessage<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsPongMessage"/> class.
        /// </summary>
        public GqltwsPongMessage()
            : base(GqltwsMessageType.PONG)
        {
        }
    }
}