// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Middleware
{
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware;

    /// <summary>
    /// A single middleware component of a context-specific pipeline.
    /// </summary>
    /// <typeparam name="TContext">The type of context processed by this pipeline.</typeparam>
    public interface IGraphMiddlewareComponent<TContext> : IGraphMiddlewareComponent
        where TContext : class, IGraphExecutionContext
    {
        /// <summary>
        /// Invokes this middleware component allowing it to perform its work against the supplied context.
        /// </summary>
        /// <param name="context">The context containing the request passed through the pipeline.</param>
        /// <param name="next">The delegate pointing to the next piece of middleware to be invoked.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns>Task.</returns>
        Task InvokeAsync(
            TContext context,
            GraphMiddlewareInvocationDelegate<TContext> next,
            CancellationToken cancelToken);
    }
}