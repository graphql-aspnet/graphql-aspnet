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
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// Executes any supplied execution directives against their relavant document parts.
    /// </summary>
    public class ExecuteDocumentDirectivesMiddleware : IQueryExecutionMiddleware
    {
        /// <inheritdoc />
        public Task InvokeAsync(
            GraphQueryExecutionContext context,
            GraphMiddlewareInvocationDelegate<GraphQueryExecutionContext> next,
            CancellationToken cancelToken)
        {
            throw new NotImplementedException();
        }
    }
}