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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Claims;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.RulesEngine.Interfaces;
    using GraphQL.AspNet.Variables;

    /// <summary>
    /// A set of information needed to successiful execute a directive as part of a field resolution.
    /// </summary>
    [DebuggerDisplay("Directive Context: {Directive.Name}")]
    public class GraphDirectiveExecutionContext : BaseGraphExecutionContext, IContextGenerator<GraphDirectiveExecutionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveExecutionContext" /> class.
        /// </summary>
        /// <param name="schema">The schema this context targets.</param>
        /// <param name="parentContext">The parent context that generated
        /// this context.</param>
        /// <param name="request">The directive request to be completed.</param>
        /// <param name="variableData">A set of variable, parsed from a query document that may be used during processing.</param>
        /// <param name="user">The user that has been preauthorized for this execution.</param>
        public GraphDirectiveExecutionContext(
            ISchema schema,
            IGraphExecutionContext parentContext,
            IGraphDirectiveRequest request,
            IResolvedVariableCollection variableData = null,
            ClaimsPrincipal user = null)
            : base(parentContext)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            this.Request = Validation.ThrowIfNullOrReturn(request, nameof(request));
            this.VariableData = variableData ?? ResolvedVariableCollection.Empty;
            this.User = user;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveExecutionContext" /> class.
        /// </summary>
        /// <param name="schema">The schema this context targets.</param>
        /// <param name="directiveRequest">The directive request to be completed.</param>
        /// <param name="operationRequest">The parent operation under which
        /// this directive execution is taking place.</param>
        /// <param name="serviceProvider">The service provider used to resolve needed
        /// objects for this context.</param>
        /// <param name="querySession">The query session.</param>
        /// <param name="userSecurityContext">The user security context from which this directive will
        /// be authorized.</param>
        /// <param name="items">A collection of developer-driven items for tracking various pieces of data.</param>
        /// <param name="metrics">The metrics package to write timing data to.</param>
        /// <param name="logger">The logger to which any event log entries will be written.</param>
        /// <param name="variableData">A set of variable, parsed from a query document that may be used during processing.</param>
        /// <param name="user">The user that has been preauthorized for this execution.</param>
        public GraphDirectiveExecutionContext(
            ISchema schema,
            IGraphDirectiveRequest directiveRequest,
            IGraphOperationRequest operationRequest,
            IServiceProvider serviceProvider,
            IQuerySession querySession,
            IUserSecurityContext userSecurityContext = null,
            MetaDataCollection items = null,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            IResolvedVariableCollection variableData = null,
            ClaimsPrincipal user = null)
            : base(
                  operationRequest,
                  serviceProvider,
                  querySession,
                  userSecurityContext,
                  items,
                  metrics,
                  logger)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            this.Request = Validation.ThrowIfNullOrReturn(directiveRequest, nameof(directiveRequest));
            this.VariableData = variableData ?? ResolvedVariableCollection.Empty;
            this.User = user;
        }

        /// <inheritdoc />
        IEnumerable<GraphDirectiveExecutionContext> IContextGenerator<GraphDirectiveExecutionContext>.CreateChildContexts()
        {
            return Enumerable.Empty<GraphDirectiveExecutionContext>();
        }

        /// <summary>
        /// Gets the target schema for this context.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }

        /// <summary>
        /// Gets the request that is being passed through this pipeline.
        /// </summary>
        /// <value>The request.</value>
        public IGraphDirectiveRequest Request { get; }

        /// <summary>
        /// Gets the directive type being targeted by this context.
        /// </summary>
        /// <value>The directive.</value>
        public IDirective Directive => this.Request?.InvocationContext?.Directive;

        /// <summary>
        /// Gets a collection of fully resolved variables for the currently executing pipeline that can be utilized in
        /// in resolving a specific field, if needed.
        /// </summary>
        /// <value>The variable data.</value>
        public IResolvedVariableCollection VariableData { get; }

        /// <summary>
        /// Gets or sets the user data authenticated and authorized on this directive
        /// execution.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; set; }
    }
}