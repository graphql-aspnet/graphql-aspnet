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
    /// A base step with commmon logic for all variable validation steps.
    /// </summary>
    /// <typeparam name="TActivePart">The expected type of the active part on a context.</typeparam>
    internal abstract class VariableValidationStep<TActivePart> : VariableValidationStep
        where TActivePart : IDocumentPart
    {
        /// <inheritdoc />
        public override bool ShouldExecute(VariableDataValidationContext context)
        {
            return context.ActivePart is TActivePart;
        }
    }
}