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

    /// <summary>
    /// A document part representing a fragment defined and spread locally in a field
    /// selection set.
    /// </summary>
    [DebuggerDisplay("Inline Fragment. Target Type: {GraphType?.Name}")]
    internal class DocumentInlineFragment : DocumentFragmentBase, IInlineFragmentDocumentPart
    {
        private bool _isIncluded;

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
        public bool IsIncluded
        {
            get
            {
                return _isIncluded;
            }

            set
            {
                _isIncluded = value;
                this.RefreshAllAscendantFields();
            }
        }

        /// <inheritdoc />
        public override string Description => "Inline Fragment";
    }
}