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
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A test middleware component in a chain. Used to test pipeline invocation order.
    /// </summary>
    public class TestMiddleware3 : IGraphFieldExecutionMiddleware
    {
        private readonly IMiddlewareTestService _testService;

        public TestMiddleware3(IMiddlewareTestService testService)
        {
            _testService = testService;
        }

        public async Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            _testService.BeforeNext(nameof(TestMiddleware3));
            await next(context, cancelToken);
            _testService.AfterNext(nameof(TestMiddleware3));
        }
    }
}