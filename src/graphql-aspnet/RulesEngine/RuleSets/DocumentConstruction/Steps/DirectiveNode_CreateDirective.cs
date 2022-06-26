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
    internal class DirectiveNode_CreateDirective : DocumentConstructionStep<DirectiveNode>
    {
        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (DirectiveNode)context.ActiveNode;

            var docPart = new DocumentDirective(context.ParentPart, node);

            var directive = context.Schema.KnownTypes.FindDirective(docPart.DirectiveName);
            docPart.AssignGraphType(directive);

            context.AssignPart(docPart);
            return true;
        }
    }
}