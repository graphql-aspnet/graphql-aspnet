// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// A fragment that was parsed out of a submitted query document.
    /// </summary>
    internal abstract class DocumentFragmentBase : DocumentPartBase, IFragmentDocumentPart, IDescendentDocumentPartSubscriber
    {
        private readonly DocumentDirectiveCollection _directives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragmentBase"/> class.
        /// </summary>
        /// <param name="parentPart">The parent document part that owns this instance.</param>
        /// <param name="sourceLocation">The location where this document part
        /// originated in the query.</param>
        public DocumentFragmentBase(IDocumentPart parentPart, SourceLocation sourceLocation)
            : base(parentPart, sourceLocation)
        {
            _directives = new DocumentDirectiveCollection(this);
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
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
        /// <param name="relativeDepth">The depth of the part relative to this part. (1 == a direct child).</param>
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
        public string TargetGraphTypeName { get; protected set; }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        /// <inheritdoc />
        public IDirectiveCollectionDocumentPart Directives => _directives;
    }
}