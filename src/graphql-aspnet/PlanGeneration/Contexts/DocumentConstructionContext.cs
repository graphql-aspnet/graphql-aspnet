// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Contexts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A subset of a document context dealing with a single operation definition within a document.
    /// </summary>
    [DebuggerDisplay("Node Type: {ActiveNode.NodeName}")]
    internal class DocumentConstructionContext : DocumentGenerationContext<SyntaxNode>, IContextGenerator<DocumentConstructionContext>
    {
        private List<SyntaxNode> _childNodes;
        private IFieldSelectionSetDocumentPart _selectionSet;
        private IDocumentPart _activePart;
        private IQueryOperationDocumentPart _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionContext" /> class.
        /// </summary>
        /// <param name="docContext">The document context.</param>
        /// <param name="node">The currently scoped node.</param>
        public DocumentConstructionContext(DocumentContext docContext, SyntaxNode node)
         : base(docContext, node)
        {
            this.BeginNewDocumentScope();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DocumentConstructionContext"/> class from being created.
        /// </summary>
        private DocumentConstructionContext()
         : base()
        {
        }

        /// <summary>
        /// Resets this node context to the scope of the provided new operation. All field sets
        /// and scopes are reset under this operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        private void BeginNewOperation(IQueryOperationDocumentPart operation)
        {
            this.DocumentContext.Operations.AddOperation(operation);
            _operation = operation;
            _selectionSet = operation.CreateFieldSelectionSet();
            this.BeginNewDocumentScope();
            this.IncreaseMaxDepth();
        }

        /// <summary>
        /// Begins a new field selection set off the active field in scope.
        /// </summary>
        public void BeginNewFieldSelectionSet()
        {
            if (!(_activePart is IFieldSelectionDocumentPart fs))
                throw new InvalidOperationException("No field currently in scope, cannot append a new selection set.");
            _selectionSet = fs.CreateFieldSelectionSet();
            this.BeginNewDocumentScope();
            this.IncreaseMaxDepth();
        }

        /// <summary>
        /// Increases the maximum depth achieved by this context.
        /// </summary>
        private void IncreaseMaxDepth()
        {
            this.MaxDepth++;
            this.DocumentContext.UpdateMaxDepth(this.MaxDepth);
        }

        /// <summary>
        /// Begins a new field scope to encapsulate a set of document parts within the current selection set.
        /// </summary>
        public void BeginNewDocumentScope()
        {
            this.DocumentScope = new DocumentScope();
        }

        /// <summary>
        /// Adds a new document part to this context, inserting where appropriate and updating the
        /// doc scope as necessary.
        /// </summary>
        /// <param name="docPart">The document part to add to this context.</param>
        public void AddDocumentPart(IDocumentPart docPart)
        {
            switch (docPart)
            {
                case IQueryOperationDocumentPart qo:
                    this.AddOrUpdateContextItemByType(qo);
                    this.BeginNewOperation(qo);
                    _activePart = qo;
                    break;

                case IQueryVariableDocumentPart qv:
                    this.AddOrUpdateContextItemByType(qv);
                    var variables = _operation?.CreateVariableCollection();
                    variables?.AddVariable(qv);
                    _activePart = qv;
                    break;

                case IFragmentDocumentPart qf:
                    this.AddOrUpdateContextItemByType(qf);
                    this.BeginNewDocumentScope();
                    break;

                case IFieldSelectionDocumentPart fs:
                    this.AddOrUpdateContextItemByType(fs);
                    _selectionSet.AddFieldSelection(fs);
                    this.DocumentScope = new DocumentScope(this.DocumentScope, fs);
                    _activePart = fs;
                    break;

                case IDirectiveDocumentPart qd:
                    // directives never alter the current scope, they just work within it
                    this.AddOrUpdateContextItemByType(qd);
                    this.DocumentScope.InsertDirective(qd);
                    _activePart = qd;
                    break;

                case IQueryArgumentDocumentPart qa:
                    if (_activePart is IQueryArgumentContainerDocumentPart argContainer)
                        argContainer.AddArgument(qa);

                    // query arguments never retain parent scopes; they are considered independent
                    this.AddOrUpdateContextItem(qa);
                    this.DocumentScope = new DocumentScope(part: qa);
                    _activePart = qa;
                    break;

                case ISuppliedValueDocumentPart qiv:
                    if (_activePart is IAssignableValueDocumentPart qia)
                        qia.AssignValue(qiv);
                    else if (_activePart is ISuppliedValueDocumentPart partQiv)
                        partQiv.AddChild(qiv);

                    this.AddOrUpdateContextItemByType<ISuppliedValueDocumentPart>(qiv);
                    _activePart = qiv;
                    break;

                default:
                    this.Messages.Critical(
                        "Unrecognized document element or position. The document element at the current position " +
                        "could not be processed and the query was terminated. Double check your document and try again.",
                        Constants.ErrorCodes.INVALID_DOCUMENT,
                        this.ActiveNode.Location.AsOrigin());
                    break;
            }
        }

        /// <summary>
        /// Adds additional <see cref="SyntaxNode" /> that this context should process as though they were
        /// direct children of it.
        /// </summary>
        /// <param name="additionalItems">The additional nodes to process.</param>
        public void AppendNodes(IEnumerable<SyntaxNode> additionalItems)
        {
            if (additionalItems != null)
            {
                _childNodes = _childNodes ?? new List<SyntaxNode>();
                _childNodes.AddRange(additionalItems);
            }
        }

        /// <summary>
        /// Creates new contexts for any chilren of the currently active context that need to be processed
        /// against the same rule set.
        /// </summary>
        /// <returns>IEnumerable&lt;TContext&gt;.</returns>
        public IEnumerable<DocumentConstructionContext> CreateChildContexts()
        {
            foreach (var child in this.ChildNodes)
            {
                // use the private constructor
                yield return new DocumentConstructionContext
                {
                    DocumentContext = this.DocumentContext,
                    Item = child,
                    ContextItems = new Dictionary<Type, IDocumentPart>(this.ContextItems),
                    DocumentScope = this.DocumentScope,
                    _activePart = this._activePart,
                    _selectionSet = this._selectionSet,
                    _operation = this._operation,
                    MaxDepth = this.MaxDepth,
                    ParentContext = this,
                };
            }
        }

        /// <summary>
        /// Gets the graph type currently in scope in the document.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphType GraphType => this.DocumentScope?.TargetGraphType ?? _selectionSet.GraphType;

        /// <summary>
        /// Gets the active node on this context.
        /// </summary>
        /// <value>The active node.</value>
        public SyntaxNode ActiveNode => this.Item;

        /// <summary>
        /// Gets the active scope, within the document, on the context.
        /// </summary>
        /// <value>The field scope.</value>
        public DocumentScope DocumentScope { get; private set; }

        /// <summary>
        /// Gets the currently scoped selection set; the physical container in the document where graph fields
        /// are placed; irrespective of the current document scope of this context.
        /// </summary>
        /// <value>The selection set.</value>
        public IFieldSelectionSetDocumentPart SelectionSet => _selectionSet;

        /// <summary>
        /// Gets the parent context that created this child context, if any. Root contexts will
        /// have a null parent.
        /// </summary>
        /// <value>The parent context.</value>
        public DocumentConstructionContext ParentContext { get; private set; }

        /// <summary>
        /// Gets a collection of nodes that should be processed as children of this context.
        /// </summary>
        /// <value>The child nodes.</value>
        internal IEnumerable<SyntaxNode> ChildNodes
        {
            get
            {
                return _childNodes != null
                    ? this.ActiveNode.Children.Concat(_childNodes)
                    : this.ActiveNode.Children;
            }
        }

        /// <summary>
        /// Gets the maximum depth of fields within the document.
        /// </summary>
        /// <value>The maximum depth.</value>
        public int MaxDepth { get; private set; }
    }
}