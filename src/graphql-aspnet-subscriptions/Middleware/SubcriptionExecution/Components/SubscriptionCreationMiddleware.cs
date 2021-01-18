// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.SubcriptionExecution.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// This middleware assemblies the final subscription if and when warranted. If assembled the query
    /// is not immediately processed.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component exists for.</typeparam>
    public class SubscriptionCreationMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        public Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context is SubcriptionExecutionContext subContext
                && subContext.IsSubscriptionOperation
                && subContext.QueryPlan != null
                && subContext.QueryOperation != null)
            {
                subContext.Subscription = new ClientSubscription<TSchema>(
                    subContext.Client,
                    subContext.Request.ToDataPackage(),
                    subContext.QueryPlan,
                    subContext.QueryOperation,
                    subContext.SubscriptionId);

                return Task.CompletedTask;
            }

            return next(context, cancelToken);
        }
    }
}