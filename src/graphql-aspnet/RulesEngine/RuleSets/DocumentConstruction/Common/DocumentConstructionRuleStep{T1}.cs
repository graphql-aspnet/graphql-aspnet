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
    using GraphQL.AspNet.PlanGeneration.Contexts;

    /// <summary>
    /// A construction step that automatically does a check for a context item.
    /// </summary>
    /// <typeparam name="T1">A required context item on a node context for this step to execute.</typeparam>
    internal abstract class DocumentConstructionRuleStep<T1> : DocumentConstructionRuleStep
        where T1 : class
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return context.Contains<T1>();
        }
    }
}