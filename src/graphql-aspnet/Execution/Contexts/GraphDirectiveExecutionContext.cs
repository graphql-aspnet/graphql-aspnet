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
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.ValidationRules.Interfaces;
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
        /// <param name="user">The user that is in scope, if any, while the directive is being executed.</param>
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
        /// <param name="serviceProvider">The service provider used to resolve needed
        /// objects for this context.</param>
        /// <param name="parentOperationRequest">The parent operation under which
        /// this directive execution is taking place.</param>
        /// <param name="directiveRequest">The directive request to be completed.</param>
        /// <param name="metrics">The metrics package to write timing data to.</param>
        /// <param name="logger">The logger to which any event log entries will be written.</param>
        /// <param name="variableData">A set of variable, parsed from a query document that may be used during processing.</param>
        /// <param name="user">The user that is in scope, if any, while the directive is being executed.</param>
        /// <param name="items">A collection of user defined items.</param>
        public GraphDirectiveExecutionContext(
            ISchema schema,
            IServiceProvider serviceProvider,
            IGraphOperationRequest parentOperationRequest,
            IGraphDirectiveRequest directiveRequest,
            IGraphQueryExecutionMetrics metrics = null,
            IGraphEventLogger logger = null,
            IResolvedVariableCollection variableData = null,
            ClaimsPrincipal user = null,
            MetaDataCollection items = null)
            : base(
                  parentOperationRequest,
                  serviceProvider,
                  null as IUserSecurityContext,
                  metrics,
                  logger,
                  items)
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
        public IDirectiveGraphType Directive => this.Request?.InvocationContext?.Directive;

        /// <summary>
        /// Gets a collection of fully resolved variables for the currently executing pipeline that can be utilized in
        /// in resolving a specific field, if needed.
        /// </summary>
        /// <value>The variable data.</value>
        public IResolvedVariableCollection VariableData { get; }

        /// <summary>
        /// Gets the user currently in scope for the request. May be null
        /// during type system construction.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; }
    }
}