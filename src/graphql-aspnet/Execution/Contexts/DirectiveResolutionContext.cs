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

    /// <summary>
    /// A context passed to a directive resolver to complete its resolution task for the field its attached to.
    /// </summary>
    [DebuggerDisplay("Directive: {Request.Directive.Name}")]
    public class DirectiveResolutionContext : BaseResolutionContext<IGraphDirectiveRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveResolutionContext"/> class.
        /// </summary>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="request">The direct request to execute.</param>
        /// <param name="arguments">The set of arguments to be passed to the directive
        /// resolver.</param>
        /// <param name="user">Optional. The user context that authenticated and authorized for this
        /// resolution context.</param>
        public DirectiveResolutionContext(
            IGraphExecutionContext parentContext,
            IGraphDirectiveRequest request,
            IExecutionArgumentCollection arguments,
            ClaimsPrincipal user = null)
            : base(parentContext, request, arguments, user)
        {
        }
    }
}