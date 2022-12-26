// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts.SuppliedValues
{
    using System;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// A base class providing common functionality for any "supplied value" to an argument
    /// or variable in a query document.
    /// </summary>
    [Serializable]
    internal abstract class DocumentSuppliedValue : DocumentPartBase, ISuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="location">The location in the source text where this
        /// document part originated.</param>
        /// <param name="key">A key value uniquely identifying this document part, if any.</param>
        protected DocumentSuppliedValue(IDocumentPart parentPart, SourceLocation location, string key = null)
            : base(parentPart, location)
        {
            this.Key = key?.Trim();
        }

        /// <inheritdoc />
        public abstract bool IsEqualTo(ISuppliedValueDocumentPart value);

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.SuppliedValue;
    }
}