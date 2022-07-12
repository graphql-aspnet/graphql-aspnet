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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// A document part representing a fragment defined and spread locally in a field
    /// selection set.
    /// </summary>
    [DebuggerDisplay("Inline Fragment. Target Type: {GraphType?.Name}")]
    internal class DocumentInlineFragment : DocumentFragmentBase<InlineFragmentNode>, IInlineFragmentDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentInlineFragment"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="fragmentNode">The inline fragment node.</param>
        public DocumentInlineFragment(IDocumentPart parentPart, InlineFragmentNode fragmentNode)
            : base(parentPart, fragmentNode)
        {
            // inline fragments are, by nature, already referenced in the document
            this.MarkAsReferenced();
            this.TargetGraphTypeName = fragmentNode.TargetType.ToString();
            this.IsIncluded = true;
        }

        /// <inheritdoc />
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            thisPath.AddFieldName("...");
            return thisPath;
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.InlineFragment;

        /// <inheritdoc />
        public bool IsIncluded
        {
            get
            {
                return this.Attributes.Contains(Constants.DocumentPartAttributes.Included);
            }

            set
            {
                if (value)
                    this.Attributes.Add(Constants.DocumentPartAttributes.Included);
                else
                    this.Attributes.Remove(Constants.DocumentPartAttributes.Included);
            }
        }

        /// <inheritdoc />
        public override string Description => "Inline Fragment";
    }
}