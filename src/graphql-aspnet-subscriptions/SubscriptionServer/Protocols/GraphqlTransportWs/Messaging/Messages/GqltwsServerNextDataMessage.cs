// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Messages
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlTransportWs.Messaging.Common;

    /// <summary>
    /// A representation of the 'NEXT' message sent when new data is available for a subscription.
    /// </summary>
    internal class GqltwsServerNextDataMessage : GqltwsMessage<IQueryExecutionResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerNextDataMessage" /> class.
        /// </summary>
        /// <param name="clientProvidedSubscriptionId">The client provided subscription identifier.</param>
        /// <param name="result">The result that needs to be sent to the client.</param>
        public GqltwsServerNextDataMessage(string clientProvidedSubscriptionId, IQueryExecutionResult result)
            : base(GqltwsMessageType.NEXT)
        {
            this.Id = clientProvidedSubscriptionId;
            this.Payload = result;
        }
    }
}