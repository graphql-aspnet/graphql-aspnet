// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Messages
{
    using System.Diagnostics;

    /// <summary>
    /// A message sent by the client when it wants to start a new subscription operation.
    /// </summary>
    [DebuggerDisplay("Apollo Subscription Start (Id: {Id})")]
    public class ApolloSubscriptionStartMessage : ApolloMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionStartMessage"/> class.
        /// </summary>
        public ApolloSubscriptionStartMessage()
            : base(ApolloMessageType.START)
        {
        }
    }
}