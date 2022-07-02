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

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

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
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this value in the user's query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        protected DocumentSuppliedValue(IDocumentPart parentPart, SyntaxNode node, string key = null)
            : base(parentPart, node)
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