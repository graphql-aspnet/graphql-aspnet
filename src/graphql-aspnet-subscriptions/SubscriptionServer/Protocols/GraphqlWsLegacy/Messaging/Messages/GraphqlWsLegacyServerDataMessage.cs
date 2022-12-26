﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Messages
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.SubscriptionServer.Protocols.GraphqlWsLegacy.Messaging.Common;

    /// <summary>
    /// A representation of the 'GQL_DATA' message sent when new data is available for the subscription.
    /// </summary>
    internal class GraphqlWsLegacyServerDataMessage : GraphqlWsLegacyMessage<IQueryExecutionResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyServerDataMessage" /> class.
        /// </summary>
        /// <param name="clientProvidedSubscriptionId">The client provided subscription identifier.</param>
        /// <param name="result">The result that needs to be sent to the client.</param>
        public GraphqlWsLegacyServerDataMessage(string clientProvidedSubscriptionId, IQueryExecutionResult result)
            : base(GraphqlWsLegacyMessageType.DATA)
        {
            this.Id = clientProvidedSubscriptionId;
            this.Payload = result;
        }
    }
}