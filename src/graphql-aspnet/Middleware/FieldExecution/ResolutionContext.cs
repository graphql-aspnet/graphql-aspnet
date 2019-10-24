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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Middleware;

    /// <summary>
    /// A base context used by all field and directive resolvers in order to successfully invoke
    /// a controller method or object property and retrieve a data value for a field.
    /// </summary>
    public abstract class ResolutionContext : BaseGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionContext" /> class.
        /// </summary>
        /// <param name="parentContext">The parent context from which this resolution context should
        /// extract is base data values.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        protected ResolutionContext(
            IGraphMiddlewareContext parentContext,
            IDataRequest request,
            IExecutionArgumentCollection arguments)
            : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(request, nameof(request));
            this.Arguments = Validation.ThrowIfNullOrReturn(arguments, nameof(arguments));
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
    }
}