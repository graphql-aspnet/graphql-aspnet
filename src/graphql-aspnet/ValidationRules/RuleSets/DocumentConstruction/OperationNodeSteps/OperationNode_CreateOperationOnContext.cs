// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.OperationNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Generates the <see cref="QueryOperation"/> to representing the operation node.
    /// </summary>
    internal class OperationNode_CreateOperationOnContext : DocumentConstructionStep<OperationTypeNode>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (OperationTypeNode)context.ActiveNode;

            var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(node.OperationType.ToString());
            if (!context.DocumentContext.Schema.OperationTypes.ContainsKey(operationType))
                return false;

            var operation = context.DocumentContext.Schema.OperationTypes[operationType];

            var queryOperation = new QueryOperation(node, operationType, operation);
            context.AddDocumentPart(queryOperation);
            return true;
        }
    }
}