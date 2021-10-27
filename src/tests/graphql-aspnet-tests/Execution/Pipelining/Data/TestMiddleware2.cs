// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Pipelining.Data
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.FieldExecution;

    public class TestMiddleware2 : IGraphFieldExecutionMiddleware
    {
        private readonly IMiddlewareTestService _testService;

        public TestMiddleware2(IMiddlewareTestService testService)
        {
            _testService = testService;
        }

        public async Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            _testService.BeforeNext(nameof(TestMiddleware2));
            await next(context, cancelToken);
            _testService.AfterNext(nameof(TestMiddleware2));
        }
    }
}