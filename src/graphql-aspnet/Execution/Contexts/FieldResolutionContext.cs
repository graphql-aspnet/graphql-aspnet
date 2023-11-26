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
    /// A context passed to a field resolver to complete its resolution task and generate data for a field.
    /// </summary>
    [GraphSkip]
    [DebuggerDisplay("Field: {Request.Field.Route.Path} (Mode = {Request.Field.Mode})")]
    public class FieldResolutionContext : SchemaItemResolutionContext<IGraphFieldRequest>
    {
        private readonly IGraphFieldRequest _fieldRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolutionContext" /> class.
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
        public FieldResolutionContext(
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            ISchema targetSchema,
            IQueryExecutionRequest queryRequest,
            IGraphFieldRequest request,
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
            _fieldRequest = request;
        }

        /// <summary>
        /// Gets or sets the resultant data object rendered by the resolver.
        /// </summary>
        /// <value>The result of executing a field's resolver.</value>
        public object Result { get; set; }

        /// <inheritdoc />
        public override ItemPath ItemPath => _fieldRequest?.Field.ItemPath;

        /// <inheritdoc />
        public override IGraphArgumentCollection SchemaDefinedArguments => _fieldRequest.Field.Arguments;

        /// <inheritdoc />
        public override object SourceData => _fieldRequest?.Data?.Value;
    }
}