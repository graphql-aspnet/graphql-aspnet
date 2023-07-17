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
    using System;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A context passed to a directive resolver to complete its resolution task for the field its attached to.
    /// </summary>
    [GraphSkip]
    [DebuggerDisplay("Directive: {Request.InvocationContext.Directive.Name}")]
    public class DirectiveResolutionContext : SchemaItemResolutionContext<IGraphDirectiveRequest>
    {
        private IGraphDirectiveRequest _directiveRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveResolutionContext" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider instance that should be used to resolve any
        /// needed services or non-schema arguments to the target resolver.</param>
        /// <param name="querySession">The query session governing the request.</param>
        /// <param name="targetSchema">The schema in scope for this resolution context.</param>
        /// <param name="queryRequest">The master query request being executed.</param>
        /// <param name="request">The resolution request to carry with the context.</param>
        /// <param name="arguments">The arguments to be passed to the resolver when its executed.</param>
        /// <param name="messages">The messages.</param>
        /// <param name="logger">(Optional) A logger instance that can be used to record scoped log entries.</param>
        /// <param name="user">(Optional) The user context that authenticated and authorized for this
        /// resolution context.</param>
        /// <param name="cancelToken">The cancel token governing the resolution of the schema item.</param>
        public DirectiveResolutionContext(
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            ISchema targetSchema,
            IQueryExecutionRequest queryRequest,
            IGraphDirectiveRequest request,
            IExecutionArgumentCollection arguments,
            IGraphMessageCollection messages = null,
            IGraphEventLogger logger = null,
            ClaimsPrincipal user = null,
            CancellationToken cancelToken = default)
            : base(
                  serviceProvider,
                  querySession,
                  targetSchema,
                  queryRequest,
                  request,
                  arguments,
                  messages,
                  logger,
                  user,
                  cancelToken)
        {
            _directiveRequest = request;
        }

        /// <inheritdoc />
        public override SchemaItemPath Route => _directiveRequest?.Directive.Route;

        /// <inheritdoc />
        public override IGraphArgumentCollection SchemaDefinedArguments => _directiveRequest?.Directive.Arguments;

        /// <inheritdoc />
        public override object SourceData => null;
    }
}