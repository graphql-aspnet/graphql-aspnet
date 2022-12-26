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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts.Common;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An wrapper for an operation syntax node to track additional details needed during the validation
    /// and construction phase.
    /// </summary>
    [DebuggerDisplay("Operation: {Name} (Type = {OperationType})")]
    internal class DocumentOperation : DocumentPartBase, IOperationDocumentPart, IDescendentDocumentPartSubscriber
    {
        private readonly DocumentVariableCollection _variableCollection = null;
        private readonly DocumentFragmentSpreadCollection _fragmentSpreads = null;
        private readonly DocumentVariableUsageCollection _variableUsages = null;
        private readonly DocumentDirectiveCollection _directives = null;
        private readonly List<IDirectiveDocumentPart> _allDirectives;
        private readonly List<ISecurableDocumentPart> _allSecuredDocParts;

        private IFieldSelectionSetDocumentPart _fieldSelectionSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentOperation"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this operation (typically the root query document).</param>
        /// <param name="operationTypeName">Name of the operation type as it was defined in the source
        /// text (this may or may not be a real operation type).</param>
        /// <param name="operationType">The operation type value parsed from the provided name, if any.</param>
        /// <param name="location">The location in the source text where this operation was declared.</param>
        /// <param name="operationName">An optional name used to alias the operation within the document.</param>
        public DocumentOperation(
           IDocumentPart parentPart,
           string operationTypeName,
           GraphOperationType operationType,
           SourceLocation location,
           string operationName = "")
       : base(parentPart, location)
        {
            this.OperationType = operationType;
            this.Name = operationName?.Trim() ?? string.Empty;
            this.OperationTypeName = operationTypeName;

            _variableCollection = new DocumentVariableCollection(this);
            _variableUsages = new DocumentVariableUsageCollection(this);
            _directives = new DocumentDirectiveCollection(this);
            _fragmentSpreads = new DocumentFragmentSpreadCollection(this);
            _allDirectives = new List<IDirectiveDocumentPart>();
            _allSecuredDocParts = new List<ISecurableDocumentPart>();
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            var isDirectChild = decendentPart.Parent == this;
            if (isDirectChild && decendentPart is IVariableDocumentPart vd)
            {
                _variableCollection.Add(vd);
            }
            else if (isDirectChild && decendentPart is IFieldSelectionSetDocumentPart fieldSelection)
            {
                _fieldSelectionSet = fieldSelection;
            }
            else if (decendentPart is IDirectiveDocumentPart ddp)
            {
                if (isDirectChild)
                    _directives.AddDirective(ddp);
                _allDirectives.Add(ddp);
            }
            else if (decendentPart is IVariableUsageDocumentPart varRef)
            {
                _variableUsages.Add(varRef);
            }
            else if (decendentPart is IFragmentSpreadDocumentPart fragSpread)
            {
                _fragmentSpreads.Add(fragSpread);
            }

            if (decendentPart is ISecurableDocumentPart sdp)
            {
                _allSecuredDocParts.Add(sdp);
            }
        }

        /// <inheritdoc />
        public GraphOperationType OperationType { get; }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart FieldSelectionSet => _fieldSelectionSet;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string OperationTypeName { get; }

        /// <inheritdoc />
        public IFragmentSpreadCollectionDocumentPart FragmentSpreads => _fragmentSpreads;

        /// <inheritdoc />
        public IVariableUsageCollectionDocumentPart VariableUsages => _variableUsages;

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Operation;

        /// <inheritdoc />
        public IVariableCollectionDocumentPart Variables => _variableCollection;

        /// <inheritdoc />
        public IDirectiveCollectionDocumentPart Directives => _directives;

        /// <inheritdoc />
        public IReadOnlyList<IDirectiveDocumentPart> AllDirectives => _allDirectives;

        /// <inheritdoc />
        public override string Description => $"Operation: {this.Name}";

        /// <inheritdoc />
        public IReadOnlyList<ISecurableDocumentPart> SecureItems => _allSecuredDocParts;
    }
}