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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Middleware;

    /// <summary>
    /// A middleware context targeting the field execution pipeline.
    /// </summary>
    [DebuggerDisplay("Field: {Field.Route.Path} (Mode = {Field.Mode})")]
    public class GraphFieldExecutionContext : BaseGraphExecutionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldExecutionContext" /> class.
        /// </summary>
        /// <param name="parentContext">The parent context.</param>
        /// <param name="fieldRequest">The field request being executed against on this pipeline context.</param>
        /// <param name="variableData">The variable data.</param>
        /// <param name="defaultFieldSources">A collection of objects to use
        /// when attempting to resolve source objects for any down stream fields.</param>
        public GraphFieldExecutionContext(
            IGraphExecutionContext parentContext,
            IGraphFieldRequest fieldRequest,
            IResolvedVariableCollection variableData,
            DefaultFieldSourceCollection defaultFieldSources = null)
             : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(fieldRequest, nameof(fieldRequest));
            this.VariableData = variableData;
            this.ResolvedSourceItems = new List<GraphDataItem>();
            this.DefaultFieldSources = defaultFieldSources ?? new DefaultFieldSourceCollection();
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

        /// <summary>
        /// Gets a collection of source objects that can, if needed, be used as the source input values to a
        /// field execution when no other sources exist.
        /// </summary>
        /// <value>The default field sources.</value>
        public DefaultFieldSourceCollection DefaultFieldSources { get; }
    }
}