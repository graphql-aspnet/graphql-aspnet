// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule to ensure the operation type defined on the document is one thats handlable
    /// by the schema.
    /// </summary>
    internal class Rule_5_2_OperationTypeMustBeDefinedOnTheSchema
        : DocumentPartValidationRuleStep<IOperationDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldAllowChildContextsToExecute(DocumentValidationContext context)
        {
            // if we cant determine the operation type we cant deteremine the root graph type
            // and cant effectively parse the query document.
            var operation = (IOperationDocumentPart)context.ActivePart;

            var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(operation.OperationTypeName.ToString());
            if (!context.Schema.Operations.ContainsKey(operationType))
                return false;

            return true;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IOperationDocumentPart)context.ActivePart;

            var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(docPart.OperationTypeName.ToString());
            if (!context.Schema.Operations.ContainsKey(operationType))
            {
                this.ValidationError(
                    context,
                    $"The target schema does not contain a '{docPart.OperationTypeName.ToString()}' operation type.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Validation.Operations";
    }
}