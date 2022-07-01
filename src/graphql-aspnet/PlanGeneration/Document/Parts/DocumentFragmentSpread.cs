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
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentPartsNew;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// Indicates that a named fragment is to be spread in place into the parent
    /// field selection set.
    /// </summary>
    [DebuggerDisplay("Spread: {FragmentName}")]
    internal class DocumentFragmentSpread : DocumentPartBase, IFragmentSpreadDocumentPart
    {
        /// <inheritdoc />
        public event DocumentCollectionAlteredHandler NamedFragmentAssigned;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentSpread" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The spread node parsed from the query document
        /// representing this instance.</param>
        public DocumentFragmentSpread(
            IDocumentPart parentPart,
            FragmentSpreadNode node)
            : base(parentPart, node)
        {
            this.FragmentName = node.PointsToFragmentName;
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
            if (targetFragment != null)
            {
                this.Fragment = targetFragment;
                this.AssignGraphType(targetFragment?.GraphType);
                targetFragment.MarkAsReferenced();
                this.NamedFragmentAssigned?.Invoke(this, new DocumentPartEventArgs(targetFragment, 0));
            }
        }

        /// <inheritdoc />
        public ReadOnlyMemory<char> FragmentName { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FragmentSpread;

        /// <inheritdoc />
        public INamedFragmentDocumentPart Fragment { get; private set; }
    }
}