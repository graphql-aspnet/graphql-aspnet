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

    /// <summary>
    /// Attempts to extract a valid query operation from the query plan on the context.
    /// </summary>
    internal class AssignQueryOperationMiddleware : IQueryExecutionMiddleware
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
            if (context.IsValid && context.QueryPlan != null && context.QueryPlan.IsValid)
            {
                context.QueryOperation = context.QueryPlan.RetrieveOperation(context.Request.OperationName);
                if (context.QueryOperation == null)
                {
                    context.Messages.Critical(
                        $"No operation found with the name '{context.Request.OperationName}'.",
                        Constants.ErrorCodes.BAD_REQUEST);
                }
            }

            return next(context, cancelToken);
        }
    }
}