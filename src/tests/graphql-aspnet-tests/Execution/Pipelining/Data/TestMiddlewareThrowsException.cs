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
    /// A middleware item that will throw an exception if invoked.
    /// </summary>
    public class TestMiddlewareThrowsException : IGraphFieldExecutionMiddleware
    {
        public class TestMiddlewareException : Exception
        {
            public TestMiddlewareException()
                : base()
            {
            }

            public TestMiddlewareException(string message)
                : base(message)
            {
            }

            public TestMiddlewareException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        public TestMiddlewareThrowsException()
        {
        }

        public Task InvokeAsync(GraphFieldExecutionContext context, GraphMiddlewareInvocationDelegate<GraphFieldExecutionContext> next, CancellationToken cancelToken)
        {
            return Task.FromException(new TestMiddlewareException());
        }
    }
}