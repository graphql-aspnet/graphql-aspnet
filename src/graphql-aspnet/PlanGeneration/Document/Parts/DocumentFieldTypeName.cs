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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A specialized implementation of a <see cref="DocumentField"/> for denoting
    /// the internal __typename metafield.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentFieldTypeName : DocumentField, IFieldTypeNameDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldTypeName"/> class.
        /// </summary>
        /// <param name="parentPart">The parent part that owns this field.</param>
        /// <param name="node">The node in the AST that defined the creation of this field.</param>
        /// <param name="field">The field referenced from the target schema.</param>
        /// <param name="fieldGraphType">The graph type for data returned from the field.</param>
        public DocumentFieldTypeName(IDocumentPart parentPart, FieldNode node, IGraphField field, IGraphType fieldGraphType)
            : base(parentPart, node, field, fieldGraphType)
        {
        }

        public DocumentFieldTypeName(
            IDocumentPart parentPart,
            IGraphField field,
            IGraphType fieldGraphType,
            SourceLocation location,
            string alias)
            : base(parentPart, field, fieldGraphType, location, alias)
        {
        }

        /// <inheritdoc />
        public override string Description => $"Field: {Constants.ReservedNames.TYPENAME_FIELD}";
    }
}