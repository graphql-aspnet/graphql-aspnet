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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An wrapper for a <see cref="OperationNode"/> to track additional details needed during the validation
    /// and construction phase.
    /// </summary>
    [DebuggerDisplay("Operation: {Name} (Type = {OperationType})")]
    internal class DocumentOperation : DocumentPartBase, IOperationDocumentPart
    {
        private DocumentVariableCollection _variableCollection = null;
        private DocumentFragmentSpreadCollection _fragmentSpreads = null;
        private DocumentVariableUsageCollection _variableUsages = null;
        private DocumentDirectiveCollection _directives = null;

        private IFieldSelectionSetDocumentPart _fieldSelectionSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentOperation" /> class.
        /// </summary>
        /// <param name="parentPart">The owning document.</param>
        /// <param name="node">The node representing the operation which created
        /// this part.</param>
        /// <param name="operationType">Type of the operation being represented.</param>
        public DocumentOperation(
            IDocumentPart parentPart,
            OperationNode node,
            GraphOperationType operationType)
            : base(parentPart, node)
        {
            this.OperationType = operationType;
            this.Name = node.OperationName.IsEmpty ? string.Empty : node.OperationName.ToString();
            this.OperationTypeName = node.OperationType.ToString();

            _variableCollection = new DocumentVariableCollection(this);
            _variableUsages = new DocumentVariableUsageCollection(this);
            _directives = new DocumentDirectiveCollection(this);
            _fragmentSpreads = new DocumentFragmentSpreadCollection(this);
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            if (relativeDepth == 1 && childPart is IVariableDocumentPart vd)
                _variableCollection.Add(vd);
            else if (relativeDepth == 1 && childPart is IFieldSelectionSetDocumentPart fieldSelection)
                _fieldSelectionSet = fieldSelection;
            else if (relativeDepth == 1 && childPart is IDirectiveDocumentPart ddp)
                _directives.AddDirective(ddp);
            else if (childPart is IVariableUsageDocumentPart varRef)
                _variableUsages.Add(varRef);
            else if (childPart is IFragmentSpreadDocumentPart fragSpread)
                _fragmentSpreads.Add(fragSpread);
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
    }
}