// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.DirectiveNodeSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Creates and assigns the active directive node to be the active <see cref="IDirectiveDocumentPart"/> on the context.
    /// </summary>
    internal class QueryDirective_CreateDirectiveOnContext : DocumentConstructionStep<DirectiveNode>
    {
        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (DirectiveNode)context.ActiveNode;

            // directive may not exist, validated in rule 5.7.1
            var directive = context.DocumentContext.Schema.KnownTypes.FindGraphType(node.DirectiveName.ToString()) as IDirective;
            var location = node.ParentNode?.AsDirectiveLocation() ?? DirectiveLocation.NONE;
            var queryDirective = new DocumentDirective(node, directive, location);
            context.AddDocumentPart(queryDirective);
            return true;
        }
    }
}