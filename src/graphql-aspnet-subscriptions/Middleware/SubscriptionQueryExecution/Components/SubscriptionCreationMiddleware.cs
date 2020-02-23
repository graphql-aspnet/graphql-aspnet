// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************


namespace GraphQL.AspNet.Middleware.QueryExecution.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware.SubscriptionCreation;

    /// <summary>
    /// This middleware assemblies the final subscription if and when warranted. If assembled the query
    /// is not immediately processed.
    /// </summary>
    public class SubscriptionCreationMiddleware : IQueryExecutionMiddleware
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
            return next(context, cancelToken);
        }
    }
}