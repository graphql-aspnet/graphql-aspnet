// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Messages.ClientMessages
{
    using System.Diagnostics;
    using GraphQL.AspNet.Messaging.Messages.Payloads;

    /// <summary>
    /// A message recieved from the client when it is notfiying the server that its dropping
    /// the connection and the server should dispose of it.
    /// </summary>
    [DebuggerDisplay("Apollo Client Terminate")]
    internal class ApolloConnectionTerminateMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloConnectionTerminateMessage"/> class.
        /// </summary>
        public ApolloConnectionTerminateMessage()
            : base(ApolloMessageType.CONNECTION_TERMINATE)
        {
        }
    }
}