// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentInputObjectField : DocumentPartBase, IInputObjectFieldDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputObjectField"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this input field.</param>
        /// <param name="suppliedName">Name of the input object as supplied in the source text.</param>
        /// <param name="field">The formal input field, from the target schema,
        /// this document part references.</param>
        /// <param name="location">The location in the source text where this field originated.</param>
        public DocumentInputObjectField(
        IDocumentPart parentPart,
        string suppliedName,
        IInputGraphField field,
        SourceLocation location)
        : base(parentPart, location)
        {
            this.Name = suppliedName;
            this.Field = field;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IInputGraphField Field { get; }

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression => this.Field?.TypeExpression;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.InputField;

        /// <inheritdoc />
        public ISuppliedValueDocumentPart Value => this
            .Children[DocumentPartType.SuppliedValue]
            .OfType<ISuppliedValueDocumentPart>()
            .FirstOrDefault();

        /// <inheritdoc />
        public override string Description => $"Input Object Field: {this.Name}";
    }
}