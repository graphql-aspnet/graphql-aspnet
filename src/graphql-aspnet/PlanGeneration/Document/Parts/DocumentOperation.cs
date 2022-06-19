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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An wrapper for a <see cref="OperationTypeNode"/> to track additional details needed during the validation
    /// and construction phase.
    /// </summary>
    [DebuggerDisplay("Operation: {Name} (Type = {OperationType})")]
    internal class DocumentOperation : IOperationDocumentPart
    {
        private readonly DocumentDirectiveCollection _rankedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentOperation" /> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="operationType">Type of the operation being represented.</param>
        /// <param name="operationGraphType">The graph type representing the operation type.</param>
        public DocumentOperation(
            OperationTypeNode node,
            GraphOperationType operationType,
            IObjectGraphType operationGraphType)
        {
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.OperationType = operationType;
            this.GraphType = Validation.ThrowIfNullOrReturn(operationGraphType, nameof(operationGraphType));
            this.Name = this.Node.OperationName.IsEmpty ? string.Empty : node.OperationName.ToString();
            _rankedDirectives = new DocumentDirectiveCollection();
        }

        /// <inheritdoc />
        public IVariableCollectionDocumentPart CreateVariableCollection()
        {
            if (this.Variables == null)
            {
                this.Variables = new DocumentVariableCollection();
            }

            return this.Variables;
        }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart CreateFieldSelectionSet()
        {
            if (this.FieldSelectionSet == null)
            {
                this.FieldSelectionSet = new DocumentFieldSelectionSet(this.GraphType, new SourcePath());
            }

            return this.FieldSelectionSet;
        }

        /// <inheritdoc />
        public void InsertDirective(IDirectiveDocumentPart directive, int rank)
        {
            _rankedDirectives.Add(rank, directive);
        }

        /// <inheritdoc />
        public GraphOperationType OperationType { get; }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart FieldSelectionSet { get; private set; }

        /// <inheritdoc />
        public OperationTypeNode Node { get; }

        /// <inheritdoc />
        public IObjectGraphType GraphType { get; }

        /// <inheritdoc />
        public IVariableCollectionDocumentPart Variables { get; private set; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var directive in this.Directives)
                    yield return directive;

                if (this.Variables != null)
                    yield return this.Variables;

                if (this.FieldSelectionSet != null)
                    yield return this.FieldSelectionSet;
            }
        }

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.Operation;

        /// <inheritdoc />
        public IEnumerable<IDirectiveDocumentPart> Directives => _rankedDirectives;
    }
}