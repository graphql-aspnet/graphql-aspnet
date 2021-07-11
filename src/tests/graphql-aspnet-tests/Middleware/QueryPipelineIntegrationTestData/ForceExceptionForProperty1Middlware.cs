// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Middleware.QueryPipelineIntegrationTestData
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.FieldExecution;

    public class ForceExceptionForProperty1Middlware : IGraphFieldExecutionMiddleware
    {
        public Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            if (context.Field.Name.Contains("property1", System.StringComparison.OrdinalIgnoreCase))
                throw new GraphExecutionException("Forced Exception for testing");

            return next(context, cancelToken);
        }
    }
}