// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common
{
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A general rule step for resolving a field <see cref="GraphDataItem"/> through a runner.
    /// </summary>
    internal abstract class FieldResolutionStep : IRuleStep<FieldValidationContext>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the TContext
        /// if it cannot process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public virtual bool ShouldExecute(FieldValidationContext context)
        {
            var status = context?.DataItem?.Status;
            return status.HasValue && !status.Value.IsFinalized();
        }

        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public abstract bool Execute(FieldValidationContext context);

        /// <summary>
        /// Determines whether the context is in a state such that it should continue processing its children, if any exist.
        /// Returning false will cease processing child items under the active item of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other items in a processing tree. This step is always executed
        /// even if the primary execution is skipped or fails.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child rulesets should be executed, <c>false</c> otherwise.</returns>
        public virtual bool ShouldAllowChildContextsToExecute(FieldValidationContext context)
        {
            return true;
        }
    }
}