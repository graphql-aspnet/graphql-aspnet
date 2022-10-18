// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.Common
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A helper class to process a single subscription event.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this instance executes against.</typeparam>
    /// <remarks>
    /// This class exists souly to reduce clutter and code in the primary
    /// proxy base.
    /// </remarks>
    internal sealed class SubscriptionEventProcessor<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IClientConnection _clientConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public SubscriptionEventProcessor(IClientConnection connection)
        {
            _clientConnection = Validation.ThrowIfNullOrReturn(connection, nameof(connection));
        }

        /// <summary>
        /// Processes the event returning the context that was executed
        /// along with its results.
        /// </summary>
        /// <param name="evt">The subscription event to process.</param>
        /// <param name="subscription">The subscription to process against.</param>
        /// <returns>GraphQueryExecutionContext.</returns>
        public async Task<(GraphQueryExecutionContext context, IGraphOperationResult result)>
            ProcessEvent(SubscriptionEvent evt, ISubscription<TSchema> subscription)
        {
            // ------------------------------
            // Setup a new execution context to process the request
            // ------------------------------
            var runtime = _clientConnection.ServiceProvider.GetRequiredService<IGraphQLRuntime<TSchema>>();
            var schema = _clientConnection.ServiceProvider.GetRequiredService<TSchema>();

            IGraphQueryExecutionMetrics metricsPackage = null;
            IGraphEventLogger logger = _clientConnection.ServiceProvider.GetService<IGraphEventLogger>();

            if (schema.Configuration.ExecutionOptions.EnableMetrics)
            {
                var factory = _clientConnection.ServiceProvider.GetRequiredService<IGraphQueryExecutionMetricsFactory<TSchema>>();
                metricsPackage = factory.CreateMetricsPackage();
            }

            var context = new GraphQueryExecutionContext(
                runtime.CreateRequest(subscription.QueryData),
                _clientConnection.ServiceProvider,
                new QuerySession(),
                securityContext: _clientConnection.SecurityContext,
                metrics: metricsPackage,
                logger: logger);

            // ------------------------------
            // register the event data as a source input for the target subscription field
            // ------------------------------
            context.DefaultFieldSources.AddSource(subscription.Field, evt.Data);
            context.QueryPlan = subscription.QueryPlan;

            // ------------------------------
            // execute the request
            // ------------------------------
            var result = await runtime.ExecuteRequest(context, _clientConnection.RequestAborted);

            return (context, result);
        }
    }
}