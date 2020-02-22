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
    /// A message sent by the client when it wants to stop an inflight subscription operation.
    /// </summary>
    ///
    [DebuggerDisplay("Apollo Subscription Stop (Id: {Id})")]
    public class ApolloSubscriptionStopMessage : ApolloMessage<ApolloNullPayload>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionStopMessage"/> class.
        /// </summary>
        public ApolloSubscriptionStopMessage()
            : base(ApolloMessageType.STOP)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionStopMessage" /> class.
        /// </summary>
        /// <param name="id">The identifier of the subscription to stop.</param>
        public ApolloSubscriptionStopMessage(string id)
            : this()
        {
            this.Id = id;
        }
    }
}