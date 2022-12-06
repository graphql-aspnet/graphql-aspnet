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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A named fragment declared at the top of a query document.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentNamedFragment : DocumentFragmentBase, INamedFragmentDocumentPart
    {
        private readonly DocumentFragmentSpreadCollection _fragmentSpreads;
        private readonly DocumentVariableUsageCollection _variableUsages;
        private readonly List<IDirectiveDocumentPart> _allDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNamedFragment"/> class.
        /// </summary>
        /// <param name="ownerDocumentPart">The document part that owns this instance.</param>
        /// <param name="fragmentName">Name of the fragment as defined in the source query.</param>
        /// <param name="targetGraphTypeName">Name of the graph type this fragment
        /// is supposed to target.</param>
        /// <param name="location">The location in the source document that generated
        /// this fragment.</param>
        public DocumentNamedFragment(
            IDocumentPart ownerDocumentPart,
            string fragmentName,
            string targetGraphTypeName,
            SourceLocation location)
            : base(ownerDocumentPart, location)
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