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
    /// A message recieved from the client after the establishment of the websocket to initialize the graphql
    /// session on the socket.
    /// </summary>
    [DebuggerDisplay("Apollo Client Initialized")]
    public class ApolloConnectionInitMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloConnectionInitMessage"/> class.
        /// </summary>
        public ApolloConnectionInitMessage()
            : base(ApolloMessageType.CONNECTION_INIT)
        {
        }
    }
}