// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Steps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Generates the <see cref="IOperationDocumentPart"/> to representing the operation node.
    /// </summary>
    internal class OperationNode_CreateOperationOnContext : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationNode_CreateOperationOnContext"/> class.
        /// </summary>
        public OperationNode_CreateOperationOnContext()
            : base(SyntaxNodeType.Operation)
        {
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;
            var operationTypeText = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();
            var operationName = context.SourceText.Slice(node.SecondaryValue.TextBlock).ToString();

            var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(operationTypeText);

            // grab a reference to the operation graph type if its
            // supported by the schema
            IGraphOperation operation = null;
            if (context.Schema.Operations.ContainsKey(operationType))
                operation = context.Schema.Operations[operationType];

            var operationPart = new DocumentOperation(
                context.ParentPart,
                operationTypeText,
                operationType,
                node.Location,
                operationName);

            operationPart.AssignGraphType(operation);

            context = context.AssignPart(operationPart);
            return true;
        }
    }
}