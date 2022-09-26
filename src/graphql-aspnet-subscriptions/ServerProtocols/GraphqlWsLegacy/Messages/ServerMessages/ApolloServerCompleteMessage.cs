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
    /// A message sent by the apollo server when a given subscription (indicated by its client provided id)
    /// will be dropped and no more data will be sent for it.
    /// </summary>
    public class ApolloServerCompleteMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloServerCompleteMessage"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public ApolloServerCompleteMessage(string id)
            : base(ApolloMessageType.COMPLETE)
        {
            this.Id = id;
        }
    }
}