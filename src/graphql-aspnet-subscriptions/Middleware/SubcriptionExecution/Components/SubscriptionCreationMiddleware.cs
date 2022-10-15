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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// This middleware assembles the final subscription when warranted. If assembled the query
    /// is not immediately processed.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component exists for.</typeparam>
    public class SubscriptionCreationMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context is SubcriptionExecutionContext subContext
                && subContext.IsSubscriptionOperation
                && subContext.QueryPlan != null)
            {
                subContext.Subscription = new ClientSubscription<TSchema>(
                    subContext.Client,
                    subContext.OperationRequest.ToDataPackage(),
                    subContext.QueryPlan,
                    subContext.SubscriptionId);

                return Task.CompletedTask;
            }

            return next(context, cancelToken);
        }
    }
}