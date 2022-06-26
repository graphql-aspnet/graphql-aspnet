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
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    /// <typeparam name="TContextItem">The context's active part must be castable to this
    /// type for the rule to execute.</typeparam>
    internal abstract class DocumentPartValidationRuleStep<TContextItem> : DocumentPartValidationRuleStep
        where TContextItem : class, IDocumentPart
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is TContextItem;
        }
    }
}