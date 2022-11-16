// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common
{
    /// <summary>
    /// A base step from which all doc construction steps must inherit to properly
    /// process a <see cref="DocumentConstructionContext"/>.
    /// </summary>
    internal abstract class BaseDocumentConstructionRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the TContext
        /// if it cannot process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified context; otherwise, <c>false</c>.</returns>
        public virtual bool ShouldExecute(DocumentConstructionContext context)
        {
            return true;
        }

        /// <summary>
        /// Executes the step the against specified TContext performing any logic or mutations according
        /// to its internal logic.
        /// </summary>
        /// <param name="context">The context encapsulating an item that needs to be processed.</param>
        /// <returns><c>true</c> if the execution completed successfully otherwise false, <c>false</c> otherwise.</returns>
        public abstract bool Execute(ref DocumentConstructionContext context);

        /// <summary>
        /// Determines where the context is in a state such that it should continue processing its children, if any exist.
        /// Returning false will cease processing child items under the active item of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other items in a processing tree. This step is always executed
        /// even if the primary execution is skipped or fails.
        /// </summary>
        /// <param name="context">The context to inspect.</param>
        /// <returns><c>true</c> if child rulesets should be executed, <c>false</c> otherwise.</returns>
        public virtual bool ShouldAllowChildContextsToExecute(DocumentConstructionContext context)
        {
            return true;
        }
    }
}