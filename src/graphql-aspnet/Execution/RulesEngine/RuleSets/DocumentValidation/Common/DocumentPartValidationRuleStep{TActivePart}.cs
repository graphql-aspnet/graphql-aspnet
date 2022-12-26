// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    /// <typeparam name="TActivePart">The context's active part must be castable to this
    /// type for the rule to execute.</typeparam>
    internal abstract class DocumentPartValidationRuleStep<TActivePart> : DocumentPartValidationRuleStep
        where TActivePart : class, IDocumentPart
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is TActivePart;
        }
    }
}