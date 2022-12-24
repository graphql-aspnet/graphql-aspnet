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
    using System.Security.Claims;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A middleware context targeting the field execution pipeline.
    /// </summary>
    [DebuggerDisplay("Field: {Field.Route.Path} (Mode = {Field.Mode})")]
    public class GraphFieldExecutionContext : MiddlewareExecutionContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldExecutionContext" /> class.
        /// </summary>
        /// <param name="parentContext">The parent context on which this field context is based.</param>
        /// <param name="fieldRequest">The field request being executed against on this pipeline context.</param>
        /// <param name="variableData">The set of resolved variables for the querry to use during
        /// field resolution.</param>
        /// <param name="defaultFieldSources">A collection of objects to use
        /// when attempting to resolve source objects for any down stream fields.</param>
        /// <param name="user">The user data used to process the execution request, if any.</param>
        /// <param name="resultCapacity">The initial capacity
        /// of the list that will contain the results from executing this context.</param>
        public GraphFieldExecutionContext(
            IMiddlewareExecutionContext parentContext,
            IGraphFieldRequest fieldRequest,
            IResolvedVariableCollection variableData,
            FieldSourceCollection defaultFieldSources = null,
            ClaimsPrincipal user = null,
            int? resultCapacity = null)
             : base(parentContext)
        {
            this.Request = Validation.ThrowIfNullOrReturn(fieldRequest, nameof(fieldRequest));
            this.VariableData = variableData;
            this.DefaultFieldSources = defaultFieldSources ?? new FieldSourceCollection();
            this.User = user;

            if (resultCapacity.HasValue)
                this.ResolvedSourceItems = new List<FieldDataItem>(resultCapacity.Value);
            else
                this.ResolvedSourceItems = new List<FieldDataItem>();
        }

        /// <summary>
        /// Gets or sets the raw, unprocessed result generated from executing
        /// (i.e. resolving) the field on this context.
        /// </summary>
        /// <value>The result.</value>
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets the user that was used to authenticate and authorize this field, if any.
        /// </summary>
        /// <value>The user.</value>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Gets the collection of source items that were successfully resolved from the generated <see cref="Result"/>.
        /// </summary>
        /// <value>The resolved source items.</value>
        public List<FieldDataItem> ResolvedSourceItems { get; }

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
        public FieldSourceCollection DefaultFieldSources { get; }
    }
}