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
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Middleware.FieldExecution;

    /// <summary>
    /// A middleware component that will be instantiated as a singleton in the test set.
    /// </summary>
    public class TestMiddlewareSingleton : IGraphFieldExecutionMiddleware
    {
        private readonly IMiddlewareTestService _testService;

        public TestMiddlewareSingleton(IMiddlewareTestService testService)
        {
            _testService = testService;
            this.Id = Guid.NewGuid().ToString("N");
        }

        public Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            _testService.BeforeNext(this.Id);
            return next(context, cancelToken);
        }

        public string Id { get; set; }
    }
}