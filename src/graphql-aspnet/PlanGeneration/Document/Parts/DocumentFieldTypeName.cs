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
    using System;
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
    }
}