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
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A document part representing a fragment defined and spread locally in a field
    /// selection set.
    /// </summary>
    [DebuggerDisplay("Inline Fragment. Target Type: {GraphType?.Name}")]
    internal class DocumentInlineFragment : DocumentFragmentBase, IInlineFragmentDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInlineFragment"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="targetType">A string identifying the graph type that
        /// restricts this inline fragment.</param>
        /// <param name="sourceLocation">The location in the source text
        /// where this fragment originated.</param>
        public DocumentInlineFragment(
            IDocumentPart parentPart,
            string targetType,
            SourceLocation sourceLocation)
            : base(parentPart, sourceLocation)
        {
            // inline fragments are, by nature, already referenced in the document
            this.MarkAsReferenced();
            this.TargetGraphTypeName = targetType;
            this.IsIncluded = true;
        }

        /// <inheritdoc />
        protected override SourcePath ExtendPath(SourcePath pathToExtend)
        {
            pathToExtend.AddFieldName("...");
            return pathToExtend;
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.InlineFragment;

        /// <inheritdoc />
        public bool IsIncluded { get; set; }

        /// <inheritdoc />
        public override string Description => "Inline Fragment";
    }
}