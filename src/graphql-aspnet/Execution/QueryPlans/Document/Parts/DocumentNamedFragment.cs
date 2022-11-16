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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// A named fragment declared at the top of a query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentNamedFragment : DocumentFragmentBase, INamedFragmentDocumentPart
    {
        private readonly DocumentFragmentSpreadCollection _fragmentSpreads;
        private readonly DocumentVariableUsageCollection _variableUsages;
        private readonly List<IDirectiveDocumentPart> _allDirectives;

        public DocumentNamedFragment(
            IDocumentPart ownerDocument,
            string fragmentName,
            string targetGraphTypeName,
            SourceLocation location)
            : base(ownerDocument, location)
        {
            this.Name = fragmentName;
            this.TargetGraphTypeName = targetGraphTypeName;

            _fragmentSpreads = new DocumentFragmentSpreadCollection(this);
            _variableUsages = new DocumentVariableUsageCollection(this);
            _allDirectives = new List<IDirectiveDocumentPart>();

        }

        /// <inheritdoc />
        protected override SourcePath ExtendPath(SourcePath pathToExtend)
        {
            pathToExtend.AddFieldName("Fragment-" + this.Name);
            return pathToExtend;
        }

        /// <inheritdoc />
        protected override void OnDecendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            base.OnDecendentPartAdded(decendentPart, relativeDepth);

            if (decendentPart is IVariableUsageDocumentPart varRef)
            {
                _variableUsages.Add(varRef);
            }
            else if (decendentPart is IFragmentSpreadDocumentPart fragSpread)
            {
                _fragmentSpreads.Add(fragSpread);
            }
            else if (decendentPart is IDirectiveDocumentPart ddp)
            {
                _allDirectives.Add(ddp);
            }
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.NamedFragment;

        /// <inheritdoc />
        public IFragmentSpreadCollectionDocumentPart FragmentSpreads => _fragmentSpreads;

        /// <inheritdoc />
        public IVariableUsageCollectionDocumentPart VariableUsages => _variableUsages;

        /// <inheritdoc />
        public IReadOnlyList<IDirectiveDocumentPart> AllDirectives => _allDirectives;

        /// <inheritdoc />
        public override string Description => $"Named Fragment: {this.Name}";
    }
}