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
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Steps;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Indicates that a named fragment is to be spread in place into the parent
    /// field selection set.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentFragmentSpread : DocumentPartBase, IFragmentSpreadDocumentPart, IDescendentDocumentPartSubscriber
    {
        private DocumentDirectiveCollection _directives;
        private bool _isIncluded;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentSpread"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="pointsToFragmentName">Name of the points to fragment.</param>
        /// <param name="location">The location in the source text where this
        /// document part originated.</param>
        public DocumentFragmentSpread(
            IDocumentPart parentPart,
            string pointsToFragmentName,
            SourceLocation location)
            : base(parentPart, location)
        {
            this.FragmentName = pointsToFragmentName;
            _directives = new DocumentDirectiveCollection(this);
            this.IsIncluded = true;
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart.Parent == this && decendentPart is IDirectiveDocumentPart ddp)
                _directives.AddDirective(ddp);
        }

        /// <inheritdoc />
        protected override SourcePath ExtendPath(SourcePath pathToExtend)
        {
            pathToExtend.AddFieldName("..." + this.FragmentName.ToString());
            return pathToExtend;
        }

        /// <inheritdoc />
        public void AssignNamedFragment(INamedFragmentDocumentPart targetFragment)
        {
            if (this.Fragment != null)
                throw new GraphExecutionException("A named fragment is already assigned and cannot be changed.");

            if (targetFragment != null)
            {
                this.Fragment = targetFragment;
                this.AssignGraphType(targetFragment?.GraphType);
                targetFragment.MarkAsReferenced();
            }
        }

        /// <inheritdoc />
        public string FragmentName { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FragmentSpread;

        /// <inheritdoc />
        public INamedFragmentDocumentPart Fragment { get; private set; }

        /// <inheritdoc />
        public IDirectiveCollectionDocumentPart Directives => _directives;

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
        public override string Description => $"Spread: {this.FragmentName}";
    }
}