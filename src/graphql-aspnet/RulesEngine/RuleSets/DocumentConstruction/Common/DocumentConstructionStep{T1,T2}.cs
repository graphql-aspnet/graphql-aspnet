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
    /// A helper base construction step that will automatically check for two different context items on a given node context before executing.
    /// </summary>
    /// <typeparam name="T1">The first of two required context items on a node context for this step to execute.</typeparam>
    /// <typeparam name="T2">The second of two required context items on a node context for this step to execute.</typeparam>
    internal abstract class DocumentConstructionStep<T1, T2> : DocumentConstructionStep<T1>
        where T1 : class
        where T2 : class
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.Contains<T2>();
        }
    }
}