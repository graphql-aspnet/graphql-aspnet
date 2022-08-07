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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A context for handling a request through the field authorization pipeline.
    /// </summary>
    [DebuggerDisplay("Auth Context, Item: {SecureSchemaItem.Route.Path}")]
    public class GraphSchemaItemSecurityContext : BaseGraphExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchemaItemSecurityContext" /> class.
        /// </summary>
        /// <param name="parentContext">The parent context which created this instance.</param>
        /// <param name="authRequest">The authorization request to be governed.</param>
        public GraphSchemaItemSecurityContext(IGraphExecutionContext parentContext, IGraphSchemaItemSecurityRequest authRequest)
             : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(authRequest, nameof(authRequest));
        }

        /// <summary>
        /// Gets or sets the security group that should be enforced
        /// during this context invocation.
        /// </summary>
        /// <value>The enforced security group.</value>
        public SchemaItemSecurityRequirements SecurityRequirements { get; set; }

        /// <summary>
        /// Gets the request that is being passed through this pipeline.
        /// </summary>
        /// <value>The request.</value>
        public IGraphSchemaItemSecurityRequest Request { get; }

        /// <summary>
        /// Gets or sets the final response generated from a middleware component as a result of executing the pipeline.
        /// </summary>
        /// <value>The response.</value>
        public SchemaItemSecurityChallengeResult Result { get; set; }

        /// <summary>
        /// Gets the secured schema item being queried with this request.
        /// </summary>
        /// <value>The field.</value>
        public ISecureSchemaItem SecureSchemaItem => this.Request.SecureSchemaItem;

        /// <summary>
        /// Gets or sets the user principal that was authenticated and will be used for
        /// authorization of the field.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal AuthenticatedUser { get; set; }
    }
}