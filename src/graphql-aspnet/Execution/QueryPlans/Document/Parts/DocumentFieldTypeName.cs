// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts
{
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Schema;

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
        /// <param name="parentPart">The document part that owns this input field.</param>
        /// <param name="field">The formal field instancne as declared in the target schema.</param>
        /// <param name="fieldGraphType">The returned field graph of the <paramref name="field"/>.</param>
        /// <param name="location">The location where this field was encountered in the source text.</param>
        /// <param name="alias">The alias supplied to this field in the source text. Use the
        /// field name if no alias was supplied.</param>
        public DocumentFieldTypeName(
            IDocumentPart parentPart,
            IGraphField field,
            IGraphType fieldGraphType,
            SourceLocation location,
            string alias)
            : base(parentPart, Constants.ReservedNames.TYPENAME_FIELD, alias, field, fieldGraphType, location)
        {
        }

        /// <inheritdoc />
        public override string Description => $"Field: {Constants.ReservedNames.TYPENAME_FIELD}";
    }
}