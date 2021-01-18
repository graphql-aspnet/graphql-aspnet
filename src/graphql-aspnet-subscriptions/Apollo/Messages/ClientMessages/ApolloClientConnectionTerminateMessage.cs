// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.Apollo.Messages.Common;
    using GraphQL.AspNet.Apollo.Messages.Payloads;

    /// <summary>
    /// A message recieved from the client when it is notfiying the server that its dropping
    /// the connection and the server should dispose of it.
    /// </summary>
    [DebuggerDisplay("Apollo Client Terminate")]
    internal class ApolloClientConnectionTerminateMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientConnectionTerminateMessage"/> class.
        /// </summary>
        public ApolloClientConnectionTerminateMessage()
            : base(ApolloMessageType.CONNECTION_TERMINATE)
        {
        }
    }
}