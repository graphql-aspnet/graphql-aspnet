// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.BidirectionalMessages
{
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// A representation of the PING message sent by a client or server to the other
    /// end of the connection.
    /// </summary>
    public class GQLWSPingMessage : GQLWSMessage<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSPingMessage"/> class.
        /// </summary>
        public GQLWSPingMessage()
            : base(GQLWSMessageType.PING)
        {
        }
    }
}