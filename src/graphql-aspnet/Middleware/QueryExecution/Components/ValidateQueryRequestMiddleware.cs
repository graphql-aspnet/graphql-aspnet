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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// Validates the data package created from the HTTP Processor before allowing the pipeline to continue.
    /// </summary>
    public class ValidateQueryRequestMiddleware : IQueryExecutionMiddleware
    {
        /// <inheritdoc />
        public Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            if (context?.ServiceProvider == null)
            {
                throw new GraphExecutionException(
                    "No context and/or service provider was supplied on which to process the request",
                    SourceOrigin.None,
                    new InvalidOperationException($"The {nameof(QueryExecutionContext)} governing the execution of the pipeline was provided as null. Operation failed."));
            }

            if (context.OperationRequest == null || string.IsNullOrWhiteSpace(context.OperationRequest.QueryText))
            {
                // capture execution exceptions, they will relate to the internal processing
                // of the server and should only be exposed to authorized parties (via exception details)
                context.Messages.Critical("No query text was provided.", Constants.ErrorCodes.EXECUTION_ERROR);
            }
            else
            {
                context.Logger?.RequestReceived(context);
            }

            return next(context, cancelToken);
        }
    }
}