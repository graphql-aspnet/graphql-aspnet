// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.NamedFragmentNodeSteps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Generates a <see cref="QueryFragment"/> out of the active named fragment node and injects it as the active
    /// item on the node context as well as adding it to the general document context.
    /// </summary>
    internal class NamedFragment_CreateFragmentOnDocument : DocumentConstructionStep<NamedFragmentNode>
    {
        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (NamedFragmentNode)context.ActiveNode;

            var fragment = new QueryFragment(node);
            context.AddDocumentPart(fragment);
            context.DocumentContext.Fragments.AddFragment(fragment);

            return true;
        }
    }
}