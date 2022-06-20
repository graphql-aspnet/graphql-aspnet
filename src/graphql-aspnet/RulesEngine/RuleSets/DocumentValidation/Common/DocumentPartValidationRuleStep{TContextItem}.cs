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

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    /// <typeparam name="TContextItem">The type of the context item to ensure exists on the context before executing the rule.</typeparam>
    internal abstract class DocumentPartValidationRuleStep<TContextItem> : DocumentPartValidationRuleStep
        where TContextItem : class
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.Contains<TContextItem>();
        }
    }
}