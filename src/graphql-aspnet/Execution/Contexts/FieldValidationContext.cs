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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A validation context used to perform final validation of this context and all its children.
    /// </summary>
    [DebuggerDisplay("{DataItem}")]
    internal class FieldValidationContext : IContextGenerator<FieldValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValidationContext" /> class.
        /// </summary>
        /// <param name="schema">The schema in scope for this validation context.</param>
        /// <param name="dataItem">The data item to process.</param>
        /// <param name="messageCollection">An optional message collection to use for this context, if not supplied
        /// a new one will be generated.</param>
        public FieldValidationContext(ISchema schema, GraphDataItem dataItem, IGraphMessageCollection messageCollection = null)
        {
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            this.DataItem = dataItem;
            this.Messages = messageCollection ?? new GraphMessageCollection();
        }

        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that need to be processed
        /// against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        public IEnumerable<FieldValidationContext> CreateChildContexts()
        {
            foreach (var child in this.DataItem.Children)
                yield return new FieldValidationContext(this.Schema, child, this.Messages);
        }

        /// <summary>
        /// Gets the schema governing the resolution of this field.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }

        /// <summary>
        /// Gets the data item in scope for this context.
        /// </summary>
        /// <value>The data item.</value>
        public GraphDataItem DataItem { get; }

        /// <summary>
        /// Gets the result data (received from a resolver) being evaluated by this context.
        /// </summary>
        /// <value>The result data.</value>
        public object ResultData => this.DataItem.ResultData;

        /// <summary>
        /// Gets the field being evaluated by this context.
        /// </summary>
        /// <value>The field.</value>
        public IGraphField Field => this.DataItem.FieldContext.Field;

        /// <summary>
        /// Gets the schema path for the field being evaluated.
        /// </summary>
        /// <value>The path.</value>
        public string FieldPath => this.Field?.Route.Path;

        /// <summary>
        /// Gets the collection of messages tracked by this context.
        /// </summary>
        /// <value>The messages.</value>
        public IGraphMessageCollection Messages { get; }

        /// <summary>
        /// Gets the type expression that should be representing the item on this context.
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression => this.DataItem.TypeExpression;

        /// <summary>
        /// Gets the origin where this instance's dataitem was generated from in the query document.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin => this.DataItem?.FieldContext?.Origin ?? SourceOrigin.None;

        /// <summary>
        /// Gets the field context the data item in this instance is part of.
        /// </summary>
        /// <value>The field context.</value>
        public IGraphFieldInvocationContext FieldContext => this.DataItem?.FieldContext;
    }
}