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

    /// <summary>
    /// A document part representing a fragment defined and spread locally in a field
    /// selection set.
    /// </summary>
    [DebuggerDisplay("Inline Fragment. Target Type: {GraphType?.Name}")]
    internal class DocumentInlineFragment : DocumentFragmentBase, IInlineFragmentDocumentPart
    {

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
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            thisPath.AddFieldName("...");
            return thisPath;
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.InlineFragment;

        /// <inheritdoc />
        public bool IsIncluded { get; set; }

        /// <inheritdoc />
        public override string Description => "Inline Fragment";
    }
}