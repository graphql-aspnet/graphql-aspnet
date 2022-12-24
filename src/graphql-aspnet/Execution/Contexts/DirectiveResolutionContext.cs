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
    using System.Diagnostics;
    using System.Security.Claims;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A context passed to a directive resolver to complete its resolution task for the field its attached to.
    /// </summary>
    [DebuggerDisplay("Directive: {Request.InvocationContext.Directive.Name}")]
    public class DirectiveResolutionContext : SchemaItemResolutionContext<IGraphDirectiveRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveResolutionContext" /> class.
        /// </summary>
        /// <param name="targetSchema">The schema in scope for this resolution context.</param>
        /// <param name="parentContext">The parent context from which this resolution context should
        /// extract is base data values.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="user">Optional. The user context that authenticated and authorized for this
        /// resolution context.</param>
        public DirectiveResolutionContext(
            ISchema targetSchema,
            IMiddlewareExecutionContext parentContext,
            IGraphDirectiveRequest request,
            IExecutionArgumentCollection arguments,
            ClaimsPrincipal user = null)
            : base(targetSchema, parentContext, request, arguments, user)
        {
        }
    }
}