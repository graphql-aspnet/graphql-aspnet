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
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Middleware;

    /// <summary>
    /// A context specific pipeline that can be invoked to perform a chain of work.
    /// </summary>
    /// <typeparam name="TContext">The type of the context the pipeline can process.</typeparam>
    public interface ISchemaPipeline<TContext> : ISchemaPipeline
        where TContext : class, IGraphExecutionContext
    {
        /// <summary>
        /// Gets the delegate function representing the start of the pipeline.
        /// </summary>
        /// <value>The delegate.</value>
        GraphMiddlewareInvocationDelegate<TContext> InvokeAsync { get; }
    }
}