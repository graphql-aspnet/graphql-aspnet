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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A base context used by all field and directive resolution contexts in order to successfully invoke
    /// a controller action, object method or object property and retrieve a data value for a field.
    /// </summary>
    public abstract class SchemaItemResolutionContext : MiddlewareExecutionContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaItemResolutionContext" /> class.
        /// </summary>
        /// <param name="targetSchema">The schema in scope for this resolution context.</param>
        /// <param name="parentContext">The parent context from which this resolution context should
        /// extract is base data values.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="user">(Optional) The user context that authenticated and authorized for this
        /// resolution context.</param>
        protected SchemaItemResolutionContext(
            ISchema targetSchema,
            IGraphQLMiddlewareExecutionContext parentContext,
            IDataRequest request,
            IExecutionArgumentCollection arguments,
            ClaimsPrincipal user = null)
            : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(request, nameof(request));
            this.Arguments = Validation.ThrowIfNullOrReturn(arguments, nameof(arguments));
            this.User = user;
            this.Schema = Validation.ThrowIfNullOrReturn(targetSchema, nameof(targetSchema));
        }

        /// <summary>
        /// Gets the set of argument, if any, to be supplied to the method the resolver will call to
        /// complete its operation.
        /// </summary>
        /// <value>The arguments.</value>
        public IExecutionArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets the request governing the resolver's operation.
        /// </summary>
        /// <value>The request.</value>
        public IDataRequest Request { get; }

        /// <summary>
        /// Gets the resolved user that was authenticated and authorized for this resolution context.
        /// May be null if no authentication or authorization took place.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets the schema that is targeted by this context.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }
    }
}