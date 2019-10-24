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
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;

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
        /// <param name="request">The request.</param>
        /// <param name="arguments">The arguments.</param>
        public DirectiveResolutionContext(
            IGraphMiddlewareContext parentContext,
            IGraphDirectiveRequest request,
            IExecutionArgumentCollection arguments)
            : base(parentContext, request, arguments)
        {
        }
    }
}