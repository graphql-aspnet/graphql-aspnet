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
        public DocumentFieldTypeName(
            IDocumentPart parentPart,
            IGraphField field,
            IGraphType fieldGraphType,
            SourceLocation location,
            string alias)
            : base(parentPart, Constants.ReservedNames.TYPENAME_FIELD, field, fieldGraphType, location, alias)
        {
        }

        /// <inheritdoc />
        public override string Description => $"Field: {Constants.ReservedNames.TYPENAME_FIELD}";
    }
}