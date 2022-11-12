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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A fragment that was parsed out of a submitted query document.
    /// </summary>
    /// <typeparam name="TSyntaxNode">The type of the syntax node from which the fragment is created.</typeparam>
    internal abstract class DocumentFragmentBase : DocumentPartBase, IFragmentDocumentPart
    {
        private readonly DocumentDirectiveCollection _directives;

        public DocumentFragmentBase(IDocumentPart parentPart, SourceLocation sourceLocation)
            : base(parentPart, sourceLocation)
        {
            _directives = new DocumentDirectiveCollection(this);
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart)
        {
            base.OnChildPartAdded(childPart);
            if (childPart.Parent == this)
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

        /// <inheritdoc />
        public IDirectiveCollectionDocumentPart Directives => _directives;
    }
}