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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A typed context used by all field and directive resolution contexts to resolve
    /// a field value.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to be resolved.</typeparam>
    public abstract class SchemaItemResolutionContext<TRequest> : SchemaItemResolutionContext
        where TRequest : class, IDataRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemResolutionContext{TRequest}" /> class.
        /// </summary>
        /// <param name="targetSchema">The schema in scope for this resolution context.</param>
        /// <param name="parentContext">The parent context from which this resolution context should
        /// extract is base data values.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="user">Optional. The user context that authenticated and authorized for this
        /// resolution context.</param>
        protected SchemaItemResolutionContext(
            ISchema targetSchema,
            IMiddlewareExecutionContext parentContext,
            TRequest request,
            IExecutionArgumentCollection arguments,
            ClaimsPrincipal user = null)
            : base(targetSchema, parentContext, request, arguments, user)
        {
        }

        /// <summary>
        /// Gets the resolution request on this context.
        /// </summary>
        /// <value>The request being resolved.</value>
        public new TRequest Request => base.Request as TRequest;
    }
}