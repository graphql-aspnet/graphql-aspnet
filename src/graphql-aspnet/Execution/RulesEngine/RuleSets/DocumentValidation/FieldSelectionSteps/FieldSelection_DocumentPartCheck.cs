// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSteps
{
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Validates that the document part representing a field selection contains valid references
    /// and is usable in the document.
    /// </summary>
    internal class FieldSelection_DocumentPartCheck : DocumentPartValidationStep<IFieldDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IFieldDocumentPart)context.ActivePart;

            // a field selection must belong to a field selection set
            if (!(docPart.Parent is IFieldSelectionSetDocumentPart))
            {
                this.ValidationError(
                  context,
                  $"The field document part '{docPart.Name}' must belong to a {typeof(IFieldSelectionSetDocumentPart).FriendlyName()} " +
                  $"but is currently attached to a {docPart.Parent.GetType().FriendlyName()}.");

                return false;
            }

            return true;
        }
    }
}