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
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// A named fragment declared at the top of a query document.
    /// </summary>
    [DebuggerDisplay("Named Fragment: {Name}")]
    internal class DocumentNamedFragment : DocumentFragmentBase<NamedFragmentNode>, INamedFragmentDocumentPart
    {
        private DocumentFragmentSpreadCollection _fragmentSpreads;
        private DocumentVariableUsageCollection _variableUsages;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentNamedFragment" /> class.
        /// </summary>
        /// <param name="ownerDocument">The query document which owns this fragment.</param>
        /// <param name="fragmentNode">The fragment node.</param>
        public DocumentNamedFragment(
            IDocumentPart ownerDocument,
            NamedFragmentNode fragmentNode)
            : base(ownerDocument, fragmentNode)
        {
            this.Name = fragmentNode.FragmentName.ToString();
            this.TargetGraphTypeName = fragmentNode.TargetType.ToString();

            _fragmentSpreads = new DocumentFragmentSpreadCollection(this);
            _variableUsages = new DocumentVariableUsageCollection(this);
        }

        /// <inheritdoc />
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            thisPath.AddFieldName("Fragment-" + this.Name);
            return thisPath;
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            base.OnChildPartAdded(childPart, relativeDepth);
            if (childPart is IVariableUsageDocumentPart varRef)
                _variableUsages.Add(varRef);
            else if (childPart is IFragmentSpreadDocumentPart fragSpread)
                _fragmentSpreads.Add(fragSpread);
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.NamedFragment;

        public IFragmentSpreadCollectionDocumentPart FragmentSpreads => _fragmentSpreads;

        public IVariableUsageCollectionDocumentPart VariableUsages => _variableUsages;
    }
}