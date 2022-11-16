// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule to ensure that the assigned fragment on a spread matches the name
    /// defined in the document.
    ///
    /// </summary>
    internal class Internal_FragmentMustMatchSuppliedName
        : DocumentPartValidationStep<IFragmentSpreadDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && ((IFragmentSpreadDocumentPart)context.ActivePart).Fragment != null;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            // its possible for the user, via directives, to swap out
            // the assigned fragment on a spread. However, the names must match
            var spread = (IFragmentSpreadDocumentPart)context.ActivePart;
            if (spread.Fragment.Name != spread.FragmentName.ToString())
            {
                this.ValidationError(
                    context,
                    $"Invalid Fragment reference. The fragment reference '{spread.Fragment.Name}' " +
                    $"does not match the expected fragment named '{spread.FragmentName.ToString()}'.");

                return false;
            }

            return true;
        }
    }
}