// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common
{
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    internal abstract class DocumentPartValidationStep : IRuleStep<DocumentValidationContext>
    {
        /// <summary>
        /// Determines where this context is in a state such that it should continue processing its children. Returning
        /// false will cease processing child nodes under the active node of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other nodes in the tree. This step is always executed
        /// even if the primary execution is skipped.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child node rulesets should be executed, <c>false</c> otherwise.</returns>
        public virtual bool ShouldAllowChildContextsToExecute(DocumentValidationContext context)
        {
            return true;
        }

        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the document part if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified document part; otherwise, <c>false</c>.</returns>
        public abstract bool ShouldExecute(DocumentValidationContext context);

        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public abstract bool Execute(DocumentValidationContext context);
    }
}