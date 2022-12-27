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
    /// Attempts to select the single operation from the parsed query document
    /// to execute.
    /// </summary>
    internal class AssignQueryOperationMiddleware : IQueryExecutionMiddleware
    {
        /// <inheritdoc />
        public Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.IsValid && context.QueryPlan == null && context.QueryDocument != null)
            {
                if (context.QueryDocument.Operations.Count == 1)
                {
                    context.Operation = context.QueryDocument.Operations[0];
                }
                else
                {
                    var operationToExecute = context.OperationRequest.OperationName?.Trim() ?? string.Empty;
                    var operation = context.QueryDocument.Operations.RetrieveOperation(operationToExecute);
                    context.Operation = operation;
                    if (context.Operation == null)
                    {
                        if (string.IsNullOrWhiteSpace(operationToExecute))
                            operationToExecute = "~anonymous~";

                        context.Messages.Critical(
                            $"Undeclared operation. An operation with the name '{operationToExecute}' was not " +
                            "found on the query document.",
                            Constants.ErrorCodes.BAD_REQUEST,
                            context.QueryDocument.SourceLocation.AsOrigin());
                    }
                }
            }

            return next(context, cancelToken);
        }
    }
}