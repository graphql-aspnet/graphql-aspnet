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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An wrapper for a <see cref="OperationNode"/> to track additional details needed during the validation
    /// and construction phase.
    /// </summary>
    [DebuggerDisplay("Operation: {Name} (Type = {OperationType})")]
    internal class DocumentOperation : DocumentPartBase, IOperationDocumentPart
    {
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
        }

        /// <inheritdoc />
        protected override SourcePath CreatePath(SourcePath path)
        {
            var thisPath = path.Clone();
            if (!string.IsNullOrWhiteSpace(this.Name))
                thisPath.AddFieldName(this.OperationType.ToString().ToLower() + "-" + this.Name.ToString());
            else
                thisPath.AddFieldName(this.OperationType.ToString().ToLower());

            return thisPath;
        }

        /// <inheritdoc />
        public IVariableCollectionDocumentPart GatherVariables()
        {
            return new DocumentVariableCollection(this);
        }

        /// <inheritdoc />
        public GraphOperationType OperationType { get; }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart FieldSelectionSet =>
            this.Children[DocumentPartType.FieldSelectionSet]
            .FirstOrDefault() as IFieldSelectionSetDocumentPart;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string OperationTypeName { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Operation;
    }
}