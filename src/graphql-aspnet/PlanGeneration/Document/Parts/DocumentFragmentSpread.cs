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
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// Indicates that a named fragment is to be spread in place into the parent
    /// field selection set.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentFragmentSpread : DocumentPartBase, IFragmentSpreadDocumentPart
    {
        /// <inheritdoc />
        public event DocumentCollectionAlteredHandler NamedFragmentAssigned;

        private DocumentDirectiveCollection _directives;


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

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            base.OnChildPartAdded(childPart, relativeDepth);
            if (relativeDepth == 1 && childPart is IDirectiveDocumentPart ddp)
            {
                _directives.AddDirective(ddp);
            }
        }

        /// <inheritdoc />
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            thisPath.AddFieldName("..." + this.FragmentName.ToString());
            return thisPath;
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
                this.NamedFragmentAssigned?.Invoke(this, new DocumentPartEventArgs(targetFragment, 0));
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
                return this.Attributes.ContainsKey(Constants.DocumentPartAttributes.IsIncluded);
            }

            set
            {
                if (value)
                    this.Attributes.TryAdd(Constants.DocumentPartAttributes.IsIncluded, value);
                else
                    this.Attributes.TryRemove(Constants.DocumentPartAttributes.IsIncluded, out _);
            }
        }

        /// <inheritdoc />
        public override string Description => $"Spread: {this.FragmentName}";
    }
}