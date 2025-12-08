// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation.Common
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// A base step with commmon logic for all variable usage validation steps.
    /// </summary>
    /// <typeparam name="TActivePart">
    /// The context's active part must be castable to this type for the rule to execute.
    /// </typeparam>
    internal abstract class VariableValidationRuleStep<TActivePart> : VariableValidationRuleStep
        where TActivePart : class, IDocumentPart
    {
        /// <inheritdoc />
        public override bool ShouldExecute(VariableDataValidationContext context)
        {
            return context.ActivePart is TActivePart;
        }
    }
}