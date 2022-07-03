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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A fragment that was parsed out of a submitted query document.
    /// </summary>
    /// <typeparam name="TSyntaxNode">The type of the syntax node from which the fragment is created.</typeparam>
    internal abstract class DocumentFragmentBase<TSyntaxNode> : DocumentPartBase<TSyntaxNode>, IFragmentDocumentPart
        where TSyntaxNode : SyntaxNode
    {
        private DocumentDirectiveCollection _directives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentBase{TSyntaxNode}" /> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this instance.</param>
        /// <param name="fragmentNode">The inline fragment node.</param>
        public DocumentFragmentBase(IDocumentPart parentPart, TSyntaxNode fragmentNode)
            : base(parentPart, fragmentNode)
        {
            _directives = new DocumentDirectiveCollection(this);
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            base.OnChildPartAdded(childPart, relativeDepth);
            if (relativeDepth == 1)
            {
                if (childPart is IFieldSelectionSetDocumentPart fss)
                {
                    this.FieldSelectionSet = fss;
                }
                else if (childPart is IDirectiveDocumentPart ddp)
                {
                    _directives.AddDirective(ddp);
                }
            }
        }

        /// <inheritdoc />
        public void MarkAsReferenced()
        {
            this.IsReferenced = true;
        }

        /// <inheritdoc />
        public bool IsReferenced { get; protected set; }

        /// <inheritdoc />
        public string TargetGraphTypeName { get; protected set; }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        public IDirectiveCollectionDocumentPart Directives => _directives;
    }
}