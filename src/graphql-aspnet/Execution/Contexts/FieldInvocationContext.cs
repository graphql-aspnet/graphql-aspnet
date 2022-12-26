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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// The default concrete representation of a field execution context.
    /// </summary>
    /// <seealso cref="IGraphFieldInvocationContext" />
    [Serializable]
    [DebuggerDisplay("Field Context: {Name} (Restict To: {ExpectedSourceTypeName}, Children = {ChildContexts.Count})")]
    public class FieldInvocationContext : IGraphFieldInvocationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldInvocationContext" /> class.
        /// </summary>
        /// <param name="schema">The schema from which this invocation context was created.</param>
        /// <param name="expectedSourceType">Expected type of the source.</param>
        /// <param name="name">The name to apply to this data set once resolution is complete.</param>
        /// <param name="field">The field.</param>
        /// <param name="fieldPart">The field document part which declared the field to be resolved.</param>
        public FieldInvocationContext(
            ISchema schema,
            Type expectedSourceType,
            string name,
            IGraphField field,
            IFieldDocumentPart fieldPart)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.Field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.FieldDocumentPart = Validation.ThrowIfNullOrReturn(fieldPart, nameof(fieldPart));
            this.ExpectedSourceType = expectedSourceType;
            this.ChildContexts = new FieldInvocationContextCollection();
            this.Arguments = new InputArgumentCollection(fieldPart.Arguments.Count);
            this.Schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <inheritdoc />
        public void Restrict(Type restrictToType)
        {
            this.ExpectedSourceType = restrictToType;
        }

        /// <inheritdoc />
        public IInputArgumentCollection Arguments { get; }

        /// <inheritdoc />
        public ISchema Schema { get; }

        /// <inheritdoc />
        public IGraphField Field { get; }

        /// <inheritdoc />
        public IFieldDocumentPart FieldDocumentPart { get; }

        /// <inheritdoc />
        public Type ExpectedSourceType { get; private set; }

        /// <summary>
        /// Gets the expected name of the source type.
        /// </summary>
        /// <value>The expected name of the source type.</value>
        private string ExpectedSourceTypeName => this.ExpectedSourceType?.FriendlyName() ?? "null";

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IFieldInvocationContextCollection ChildContexts { get; }

        /// <inheritdoc />
        public SourceLocation Location => this.FieldDocumentPart.SourceLocation;

        /// <inheritdoc />
        public SourceOrigin Origin => this.FieldDocumentPart.Origin;
    }
}