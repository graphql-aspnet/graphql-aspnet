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

    /// <summary>
    /// A message sent by the client when it wants to start a new subscription operation.
    /// </summary>
    [DebuggerDisplay("Apollo Subscription Start (Id: {Id})")]
    public class ApolloClientStartMessage : ApolloMessage<GraphQueryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientStartMessage"/> class.
        /// </summary>
        public ApolloClientStartMessage()
            : base(ApolloMessageType.START)
        {
        }
    }
}