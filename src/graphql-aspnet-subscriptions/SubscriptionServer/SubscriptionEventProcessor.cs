// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A helper class used to process a single subscription event for a single connected client proxy.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this instance executes against.</typeparam>
    /// <remarks>
    /// This class acts as a helper class for encapsulating common functionality relating to
    /// sending subscriptions through the primary runtime.
    /// </remarks>
    public sealed class SubscriptionEventProcessor<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventProcessor{TSchema}" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider owned by the client that it uses
        /// to create components.</param>
        public SubscriptionEventProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Processes the event returning the context that was executed
        /// along with its results.
        /// </summary>
        /// <param name="securityContext">The security context to use during the execution process.</param>
        /// <param name="evt">The subscription event to process.</param>
        /// <param name="subscription">The subscription to process the event against.</param>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The execution context that was processed through the graphql runtime.</returns>
        public async Task<QueryExecutionContext> ProcessEventAsync(
            IUserSecurityContext securityContext,
            SubscriptionEvent evt,
            ISubscription<TSchema> subscription,
            CancellationToken cancelToken = default)
        {
            // ------------------------------
            // Setup a new execution context to process the request
            // ------------------------------
            var runtime = _serviceProvider.GetRequiredService<IGraphQLRuntime<TSchema>>();
            var schema = _serviceProvider.GetRequiredService<TSchema>();

            IQueryExecutionMetrics metricsPackage = null;
            IGraphEventLogger logger = _serviceProvider.GetService<IGraphEventLogger>();

            if (schema.Configuration.ExecutionOptions.EnableMetrics)
            {
                var factory = _serviceProvider.GetRequiredService<IQueryExecutionMetricsFactory<TSchema>>();
                metricsPackage = factory.CreateMetricsPackage();
            }

            var context = new QueryExecutionContext(
                runtime.CreateRequest(subscription.QueryData),
                _serviceProvider,
                new QuerySession(),
                securityContext: securityContext,
                metrics: metricsPackage,
                logger: logger);

            // ------------------------------
            // register the subscription event data as a source input for the target subscription field
            // ------------------------------
            context.DefaultFieldSources.AddSource(subscription.Field, evt.Data);
            context.QueryPlan = subscription.QueryPlan;

            // ------------------------------
            // execute the request
            // ------------------------------
            await runtime.ExecuteRequestAsync(context, cancelToken);
            return context;
        }
    }
}