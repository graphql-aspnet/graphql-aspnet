// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// A representation of the 'NEXT' message sent when new data is available for a subscription.
    /// </summary>
    public class GQLWSServerNextDataMessage : GQLWSMessage<IGraphOperationResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GQLWSServerNextDataMessage" /> class.
        /// </summary>
        /// <param name="clientProvidedSubscriptionId">The client provided subscription identifier.</param>
        /// <param name="result">The result that needs to be sent to the client.</param>
        public GQLWSServerNextDataMessage(string clientProvidedSubscriptionId, IGraphOperationResult result)
            : base(GQLWSMessageType.NEXT)
        {
            this.Id = clientProvidedSubscriptionId;
            this.Payload = result;
        }
    }
}