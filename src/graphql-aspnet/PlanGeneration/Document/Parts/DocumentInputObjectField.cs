// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An input argument (on a field or directive) that defined in a user's query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentInputObjectField : DocumentPartBase, IInputObjectFieldDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInputObjectField" /> class.
        /// </summary>
        /// <param name="parentPart">The document part(a field or directive) that owns this argument.</param>
        /// <param name="node">The node in the query document creating this entity.</param>
        /// <param name="field">The field of data on an INPUT_OBJECT represented by this
        /// document part.</param>
        public DocumentInputObjectField(
            IDocumentPart parentPart,
            InputItemNode node,
            IInputGraphField field)
            : base(parentPart, node)
        {
            this.Name = node.InputName.ToString();
            this.Field = field;
        }

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