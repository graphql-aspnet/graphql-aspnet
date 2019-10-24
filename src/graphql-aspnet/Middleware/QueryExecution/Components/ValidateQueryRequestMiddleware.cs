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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// Validates the data package created from the HTTP Processor before allowing the pipeline to continue.
    /// </summary>
    public class ValidateQueryRequestMiddleware : IQueryExecutionMiddleware
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
            if (context?.ServiceProvider == null)
            {
                throw new GraphExecutionException(
                    "No context and/or service provider was supplied on which to process the request",
                    SourceOrigin.None,
                    new InvalidOperationException($"The {nameof(GraphQueryExecutionContext)} governing the execution of the pipeline was provided as null. Operation failed."));
            }

            if (context.Request == null || string.IsNullOrWhiteSpace(context.Request.QueryText))
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