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
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.Schema;

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
        /// <param name="fieldName">Name of the field as declared in the query text.</param>
        /// <param name="alias">The alias applied to this field. Value should be set
        /// to the <paramref name="fieldName"/> if no formal alias was supplied.</param>
        /// <param name="field">The field as its defined in the target schema.</param>
        /// <param name="fieldGraphType">The qualified graph type returned by the field.</param>
        /// <param name="location">The location in the source text where the field
        /// originated.</param>
        public DocumentField(
            IDocumentPart parentPart,
            string fieldName,
            string alias,
            IGraphField field,
            IGraphType fieldGraphType,
            SourceLocation location)
            : base(parentPart, fieldName, alias, field, fieldGraphType, location)
        {
        }

        /// <inheritdoc />
        public override string Description => $"Field: {this.Field?.Name ?? "-unknown-"}";
    }
}