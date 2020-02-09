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
    /// A context passed to a field resolver to complete its resolution task and generate data for a field.
    /// </summary>
    [DebuggerDisplay("Field: {Request.Field.Route.Path} (Mode = {Request.Field.Mode})")]
    public class FieldResolutionContext : BaseResolutionContext<IGraphFieldRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolutionContext"/> class.
        /// </summary>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="request">The request.</param>
        /// <param name="arguments">The arguments.</param>
        public FieldResolutionContext(
            IGraphMiddlewareContext parentContext,
            IGraphFieldRequest request,
            IExecutionArgumentCollection arguments)
            : base(parentContext, request, arguments)
        {
        }

        /// <summary>
        /// Gets or sets the resultant data object created by the resolver.
        /// </summary>
        /// <value>The result.</value>
        public object Result { get; set; }
    }
}