﻿// *************************************************************
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
    using System.Xml.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An wrapper for a <see cref="OperationNode"/> to track additional details needed during the validation
    /// and construction phase.
    /// </summary>
    [DebuggerDisplay("Operation: {Name} (Type = {OperationType})")]
    internal class DocumentOperation : DocumentPartBase, IOperationDocumentPart
    {
        private readonly DocumentVariableCollection _variableCollection = null;
        private readonly DocumentFragmentSpreadCollection _fragmentSpreads = null;
        private readonly DocumentVariableUsageCollection _variableUsages = null;
        private readonly DocumentDirectiveCollection _directives = null;
        private readonly List<IDirectiveDocumentPart> _allDirectives;
        private readonly List<ISecureDocumentPart> _allSecuredDocParts;

        private IFieldSelectionSetDocumentPart _fieldSelectionSet;

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
            _allSecuredDocParts = new List<ISecureDocumentPart>();

        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart)
        {
            var isDirectChild = childPart.Parent == this;
            if (isDirectChild && childPart is IVariableDocumentPart vd)
            {
                _variableCollection.Add(vd);
            }
            else if (isDirectChild && childPart is IFieldSelectionSetDocumentPart fieldSelection)
            {
                _fieldSelectionSet = fieldSelection;
            }
            else if (childPart is IDirectiveDocumentPart ddp)
            {
                if (isDirectChild)
                    _directives.AddDirective(ddp);
                _allDirectives.Add(ddp);
            }
            else if (childPart is IVariableUsageDocumentPart varRef)
            {
                _variableUsages.Add(varRef);
            }
            else if (childPart is IFragmentSpreadDocumentPart fragSpread)
            {
                _fragmentSpreads.Add(fragSpread);
            }

            if (childPart is ISecureDocumentPart sdp)
                _allSecuredDocParts.Add(sdp);
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
        public int MaxDepth { get; private set; }

        /// <inheritdoc />
        public override string Description => $"Operation: {this.Name}";

        /// <inheritdoc />
        public IReadOnlyList<ISecureDocumentPart> SecureItems => _allSecuredDocParts;
    }
}