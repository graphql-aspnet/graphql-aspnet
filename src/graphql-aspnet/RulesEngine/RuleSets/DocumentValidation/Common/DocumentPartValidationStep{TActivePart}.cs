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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.Interfaces;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    /// <typeparam name="TActivePart">The expected type of the active part on a context.</typeparam>
    internal abstract class DocumentPartValidationStep<TActivePart> : DocumentPartValidationStep
        where TActivePart : IDocumentPart
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is TActivePart;
        }
    }
}