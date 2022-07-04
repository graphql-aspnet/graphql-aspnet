// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Steps
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Assigns the active query fragment on the current context to be the fragment pointed to by the spread.
    /// </summary>
    internal class FragmentSpread_RegisterNamedFragmentToContext : DocumentConstructionStep<FragmentSpreadNode>
    {
        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FragmentSpreadNode)context.ActiveNode;
            var docPart = new DocumentFragmentSpread(context.ParentPart, node);

            // cant set graphtype yet, that is done during linking after all named
            // fragments are parsed
            context.AssignPart(docPart);
            return true;
        }
    }
}