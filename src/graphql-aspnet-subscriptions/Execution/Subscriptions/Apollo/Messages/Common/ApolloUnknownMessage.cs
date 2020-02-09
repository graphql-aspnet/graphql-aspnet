// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common
{
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Payloads;

    /// <summary>
    /// A message representing an unknown message type.
    /// </summary>
    public class ApolloUnknownMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloUnknownMessage"/> class.
        /// </summary>
        public ApolloUnknownMessage()
            : base(ApolloMessageType.UNKNOWN)
        {
        }
    }
}