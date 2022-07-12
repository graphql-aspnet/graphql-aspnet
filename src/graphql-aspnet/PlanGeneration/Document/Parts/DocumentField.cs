// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A single field of data requested on a user's query document.
    /// </summary>
    [DebuggerDisplay("Field: {Field?.Name} (Returns: {GraphType?.Name})")]
    internal class DocumentField : DocumentFieldBase, IFieldDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentField" /> class.
        /// </summary>
        /// <param name="parentPart">The parent part that owns this field.</param>
        /// <param name="node">The node representing the field in the query document.</param>
        /// <param name="field">The field as its defined in the target schema.</param>
        /// <param name="fieldGraphType">The qualified graph type returned by the field.</param>
        public DocumentField(IDocumentPart parentPart, FieldNode node, IGraphField field, IGraphType fieldGraphType)
            : base(parentPart, node, field, fieldGraphType)
        {
        }

        public override string Description => $"Field: {Field?.Name ?? "-unknown-"}";
    }
}