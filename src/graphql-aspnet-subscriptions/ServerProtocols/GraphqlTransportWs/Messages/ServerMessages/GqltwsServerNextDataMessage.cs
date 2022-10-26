// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;

    /// <summary>
    /// A representation of the 'NEXT' message sent when new data is available for a subscription.
    /// </summary>
    public class GqltwsServerNextDataMessage : GqltwsMessage<IGraphOperationResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsServerNextDataMessage" /> class.
        /// </summary>
        /// <param name="clientProvidedSubscriptionId">The client provided subscription identifier.</param>
        /// <param name="result">The result that needs to be sent to the client.</param>
        public GqltwsServerNextDataMessage(string clientProvidedSubscriptionId, IGraphOperationResult result)
            : base(GqltwsMessageType.NEXT)
        {
            this.Id = clientProvidedSubscriptionId;
            this.Payload = result;
        }
    }
}