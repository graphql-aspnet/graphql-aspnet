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
    internal abstract class DocumentFragmentBase : DocumentPartBase, IFragmentDocumentPart, IDecdendentDocumentPartSubscriber
    {
        private readonly DocumentDirectiveCollection _directives;

        public DocumentFragmentBase(IDocumentPart parentPart, SourceLocation sourceLocation)
            : base(parentPart, sourceLocation)
        {
            _directives = new DocumentDirectiveCollection(this);
        }

        /// <inheritdoc cref="IDecdendentDocumentPartSubscriber.OnDecendentPartAdded" />
        void IDecdendentDocumentPartSubscriber.OnDecendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            this.OnDecendentPartAdded(decendentPart, relativeDepth);
        }

        /// <summary>
        /// When overriden in a child class, this called when a new document
        /// part has been added to this instance as a decendent.
        /// </summary>
        /// <remarks>
        /// All implementers should invoke the base version before processing
        /// any information.
        /// </remarks>
        /// <param name="decendentPart">The decendent part that was added.</param>
        /// <param name="relativeDepth">The depth of the part relative to this part. (1 == a direct child)</param>
        protected virtual void OnDecendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart.Parent == this)
            {
                if (decendentPart is IFieldSelectionSetDocumentPart fss)
                {
                    this.FieldSelectionSet = fss;
                }
                else if (decendentPart is IDirectiveDocumentPart ddp)
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