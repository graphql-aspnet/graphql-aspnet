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
    /// A message sent by the server to a client to acknowledge receipt of a message when no other
    /// specific message is warranted.
    /// </summary>
    public class ApolloServerAckOperationMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloServerAckOperationMessage"/> class.
        /// </summary>
        public ApolloServerAckOperationMessage()
            : base(ApolloMessageType.CONNECTION_ACK)
        {
        }
    }
}