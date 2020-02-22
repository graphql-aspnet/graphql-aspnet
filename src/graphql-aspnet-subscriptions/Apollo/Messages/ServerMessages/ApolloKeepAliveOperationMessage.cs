// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.ServerMessages
{
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.Payloads;

    /// <summary>
    /// A keep alive message sent periodically by the server to keep the connection
    /// open a the application level.
    /// </summary>
    public class ApolloKeepAliveOperationMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloKeepAliveOperationMessage"/> class.
        /// </summary>
        public ApolloKeepAliveOperationMessage()
            : base(ApolloMessageType.CONNECTION_KEEP_ALIVE)
        {
        }
    }
}