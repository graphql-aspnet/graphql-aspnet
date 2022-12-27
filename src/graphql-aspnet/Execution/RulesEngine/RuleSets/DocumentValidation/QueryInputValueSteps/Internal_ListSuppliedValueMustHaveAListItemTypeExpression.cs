// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Ensures that any supplied list of values was correctly assigned an expected
    /// type expresion for the items in the list.
    /// </summary>
    internal class Internal_ListSuppliedValueMustHaveAListItemTypeExpression
        : DocumentPartValidationStep<IListSuppliedValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var listValue = (IListSuppliedValueDocumentPart)context.ActivePart;

            if (listValue.ListItemTypeExpression == null)
            {
                this.ValidationError(
                  context,
                  $"Invalid or unexpected list of items. No " +
                  $"type expression could be determined for the items within the list.");

                return false;
            }

            return true;
        }
    }
}