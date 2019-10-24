// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.Interfaces
{
    /// <summary>
    /// A Rule that can be executed by a <see cref="RuleProcessor{TContext}"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the rule context to execute against.</typeparam>
    internal interface IRuleStep<TContext>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the TContext
        /// if it cannot process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        bool ShouldExecute(TContext context);

        /// <summary>
        /// Executes the step the against specified TContext performing any logic or mutations according
        /// to its internal logic.
        /// </summary>
        /// <param name="context">The validation context encapsulating an item that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        bool Execute(TContext context);

        /// <summary>
        /// Determines where the context is in a state such that it should continue processing its children, if any exist.
        /// Returning false will cease processing child items under the active item of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other items in a processing tree. This step is always executed
        /// even if the primary execution is skipped or fails.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child rulesets should be executed, <c>false</c> otherwise.</returns>
        bool ShouldAllowChildContextsToExecute(TContext context);
    }
}