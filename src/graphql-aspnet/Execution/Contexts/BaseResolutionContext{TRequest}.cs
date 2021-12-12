// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Contexts
{
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Execution;

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
        /// <param name="parentContext">The parent context from which this resolution context should
        /// extract is base data values.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="user">Optional. The user context that authenticated and authorized for this
        /// resolution context.</param>
        protected BaseResolutionContext(
            IGraphExecutionContext parentContext,
            TRequest request,
            IExecutionArgumentCollection arguments,
            ClaimsPrincipal user = null)
            : base(parentContext, request, arguments, user)
        {
        }

        /// <summary>
        /// Gets the resolution request on this context.
        /// </summary>
        /// <value>The request.</value>
        public new TRequest Request => base.Request as TRequest;
    }
}