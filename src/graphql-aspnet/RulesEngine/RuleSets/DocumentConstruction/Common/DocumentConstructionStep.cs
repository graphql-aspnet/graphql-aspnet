// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common
{
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A base construction step containing common logic for most active steps (not validators).
    /// </summary>
    internal abstract class DocumentConstructionStep : IRuleStep<DocumentConstructionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentConstructionStep"/> class.
        /// </summary>
        protected DocumentConstructionStep()
        {
        }

        /// <summary>
        /// Determines where this context is in a state such that it should continue processing its children. Returning
        /// false will cease processing child nodes under the active node of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other nodes in the tree. This step is always executed
        /// even if the primary execution is skipped.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child node rulesets should be executed, <c>false</c> otherwise.</returns>
        public virtual bool ShouldAllowChildContextsToExecute(DocumentConstructionContext context)
        {
            return true;
        }

        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public abstract bool ShouldExecute(DocumentConstructionContext context);

        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public abstract bool Execute(DocumentConstructionContext context);
    }
}