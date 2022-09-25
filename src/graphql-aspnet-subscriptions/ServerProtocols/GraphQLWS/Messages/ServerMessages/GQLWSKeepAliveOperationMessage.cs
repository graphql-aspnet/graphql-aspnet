// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages
{
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Payloads;

    /// <summary>
    /// A keep alive message sent periodically by the server to keep the connection
    /// open a the application level.
    /// </summary>
    public class GQLWSKeepAliveOperationMessage : GQLWSMessage<GQLWSNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSKeepAliveOperationMessage"/> class.
        /// </summary>
        public GQLWSKeepAliveOperationMessage()
            : base(GQLWSMessageType.CONNECTION_KEEP_ALIVE)
        {
        }
    }
}