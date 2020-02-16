// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A helper class that completes the sending of any new subscription data to a client.
    /// </summary>
    internal class ApolloSubscriptionNewDataHandler
    {
        private readonly ISubscription _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionNewDataHandler" /> class.
        /// </summary>
        /// <param name="client">The client to recieve data.</param>
        public ApolloSubscriptionNewDataHandler(ISubscription client)
        {
            _client = Validation.ThrowIfNullOrReturn(client, nameof(client));
        }

        /// <summary>
        /// Processes the data result and sends the appropriate response to the connected client.
        /// </summary>
        /// <param name="operationResult">The operation result.</param>
        /// <returns>Task.</returns>
        public Task SendNewData(IGraphOperationResult operationResult)
        {
            return Task.CompletedTask;
        }
    }
}