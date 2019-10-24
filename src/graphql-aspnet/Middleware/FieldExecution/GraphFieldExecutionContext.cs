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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A middleware context targeting the field execution pipeline.
    /// </summary>
    [DebuggerDisplay("Field: {Field.Route.Path} (Mode = {Field.Mode})")]
    public class GraphFieldExecutionContext : BaseGraphMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldExecutionContext" /> class.
        /// </summary>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="fieldRequest">The field request being executed against on this pipeline context.</param>
        /// <param name="variableData">The variable data.</param>
        public GraphFieldExecutionContext(
            IGraphMiddlewareContext parentContext,
            IGraphFieldRequest fieldRequest,
            IResolvedVariableCollection variableData)
             : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(fieldRequest, nameof(fieldRequest));
            this.VariableData = variableData;
            this.ResolvedSourceItems = new List<GraphDataItem>();
        }

        /// <summary>
        /// Gets or sets the raw result generated of executing the field on this context.
        /// </summary>
        /// <value>The result.</value>
        public object Result { get; set; }

        /// <summary>
        /// Gets the collection of source items that were successfully resolved from the generated <see cref="Result"/>.
        /// </summary>
        /// <value>The resolved source items.</value>
        public List<GraphDataItem> ResolvedSourceItems { get; }

        /// <summary>
        /// Gets the request that is being passed through this pipeline.
        /// </summary>
        /// <value>The request.</value>
        public IGraphFieldRequest Request { get; }

        /// <summary>
        /// Gets the invocation context.
        /// </summary>
        /// <value>The invocation context.</value>
        public IGraphFieldInvocationContext InvocationContext => this.Request.InvocationContext;

        /// <summary>
        /// Gets a collection of fully resolved variables for the currently executing pipeline that can be utilized in
        /// in resolving a specific field, if needed.
        /// </summary>
        /// <value>The variable data.</value>
        public IResolvedVariableCollection VariableData { get; }

        /// <summary>
        /// Gets the field in scope for the request being passed through the pipeline.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field => this.Request.InvocationContext?.Field;
    }
}