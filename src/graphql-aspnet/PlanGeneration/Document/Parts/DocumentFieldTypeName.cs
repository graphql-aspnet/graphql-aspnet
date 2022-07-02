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
    [DebuggerDisplay("Field: __typename (Returns: {GraphType?.Name})")]
    internal class DocumentFieldTypeName : DocumentFieldBase, IFieldTypeNameDocumentPart
    {
        public DocumentFieldTypeName(IDocumentPart parentPart, FieldNode node, IGraphField field, IGraphType fieldGraphType)
            : base(parentPart, node, field, fieldGraphType)
        {
        }
    }
}