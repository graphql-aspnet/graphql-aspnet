// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues
{
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// An input value representing that nothing/null was used as a supplied parameter for an input argument.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentNullSuppliedValue : DocumentSuppliedValue, INullSuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNullSuppliedValue"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="location">The location in the source text where this
        /// document part originated.</param>
        /// <param name="key">A key value uniquely identifying this document part, if any.</param>
        public DocumentNullSuppliedValue(IDocumentPart parentPart, SourceLocation location, string key = null)
            : base(parentPart, location, key)
        {
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            return value != null && value is INullSuppliedValueDocumentPart;
        }

        /// <inheritdoc />
        public override string Description => "Null Value";
    }
}