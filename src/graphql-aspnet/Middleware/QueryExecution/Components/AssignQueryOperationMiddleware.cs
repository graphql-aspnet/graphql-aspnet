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
    /// Attempts to extract a valid query operation from the query plan on the context.
    /// </summary>
    internal class AssignQueryOperationMiddleware : IQueryExecutionMiddleware
    {
        /// <inheritdoc />
        public Task InvokeAsync(GraphQueryExecutionContext context, GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.QueryPlan != null && context.QueryPlan.IsValid)
            {
                context.QueryOperation = context.QueryPlan.RetrieveOperation(context.OperationRequest.OperationName);
                if (context.QueryOperation == null)
                {
                    context.Messages.Critical(
                        $"No operation found with the name '{context.OperationRequest.OperationName}'.",
                        Constants.ErrorCodes.BAD_REQUEST);
                }
            }

            return next(context, cancelToken);
        }
    }
}