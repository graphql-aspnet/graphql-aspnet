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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A context for handling a request through the field authorization pipeline.
    /// </summary>
    [DebuggerDisplay("Auth Context, Field: {Field.Route.Path}")]
    public class GraphFieldAuthorizationContext : BaseGraphExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldAuthorizationContext" /> class.
        /// </summary>
        /// <param name="parentContext">The parent context which created this instance.</param>
        /// <param name="authRequest">The authorization request to be governed.</param>
        public GraphFieldAuthorizationContext(IGraphExecutionContext parentContext, IGraphFieldAuthorizationRequest authRequest)
             : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(authRequest, nameof(authRequest));
        }

        /// <summary>
        /// Gets the request that is being passed through this pipeline.
        /// </summary>
        /// <value>The request.</value>
        public IGraphFieldAuthorizationRequest Request { get; }

        /// <summary>
        /// Gets or sets the response generated from a middleware component as a result of executing the pipeline.
        /// </summary>
        /// <value>The response.</value>
        public FieldAuthorizationResult Result { get; set; }

        /// <summary>
        /// Gets the field in scope for the request being passed through the pipeline.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field => this.Request.Field;
    }
}