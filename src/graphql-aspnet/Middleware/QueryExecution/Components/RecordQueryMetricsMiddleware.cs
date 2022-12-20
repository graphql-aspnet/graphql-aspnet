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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// A middleware component for tracking execution time and other Apollo tracing
    /// compatiable metrics: <see href="https://github.com/apollographql/apollo-tracing" /> .
    /// </summary>
    public class RecordQueryMetricsMiddleware : IQueryExecutionMiddleware
    {
        /// <inheritdoc />
        public async Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            context?.Metrics?.Start();
            await next(context, cancelToken).ConfigureAwait(false);
            context?.Metrics?.End();
        }
    }
}