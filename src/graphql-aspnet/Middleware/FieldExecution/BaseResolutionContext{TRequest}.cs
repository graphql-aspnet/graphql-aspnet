// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Middleware.FieldExecution
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;

    /// <summary>
    /// A base set of options used by all resolution-scoped contexts in the library.
    /// </summary>
    /// <typeparam name="TRequest">The type of the t request.</typeparam>
    public abstract class BaseResolutionContext<TRequest> : ResolutionContext
        where TRequest : class, IDataRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseResolutionContext{TRequest}"/> class.
        /// </summary>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="request">The request.</param>
        /// <param name="arguments">The arguments.</param>
        protected BaseResolutionContext(
            IGraphMiddlewareContext parentContext,
            TRequest request,
            IExecutionArgumentCollection arguments)
            : base(parentContext, request, arguments)
        {
        }

        /// <summary>
        /// Gets the resolution request on this context.
        /// </summary>
        /// <value>The request.</value>
        public new TRequest Request => base.Request as TRequest;
    }
}