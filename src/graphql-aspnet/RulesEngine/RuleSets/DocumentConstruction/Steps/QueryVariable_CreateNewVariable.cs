// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.VariableNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Adds a new variable to the current context and active variable collection.
    /// </summary>
    internal class QueryVariable_CreateNewVariable : DocumentConstructionStep<VariableNode>
    {
        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (VariableNode)context.ActiveNode;

            var docPart = new DocumentVariable(context.ParentPart, node);
            var graphType = context.Schema.KnownTypes.FindGraphType(docPart.TypeExpression.TypeName);
            docPart.AssignGraphType(graphType);

            context.AssignPart(docPart);
            return true;
        }
    }
}