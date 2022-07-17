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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An internal rule to ensure that the graph type assigned to this
    /// operation matches the requested operation type.
    /// </summary>
    internal class Internal_OperationAssignedGraphTypeMustMatchExpectedOperationType
        : DocumentPartValidationStep<IOperationDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && context?.ActivePart?.GraphType != null;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var operationPart = (IOperationDocumentPart)context.ActivePart;
            var operation = operationPart.GraphType as IGraphOperation;

            if (operation == null || operation.OperationType != operationPart.OperationType)
            {
                var name = operation?.OperationType ?? GraphOperationType.Unknown;
                var nameString = name.ToString().ToLower();

                this.ValidationError(
                    context,
                    $"Invalid referenced operation. The referenced operational graph type '{nameString}' does not match the expected graph type " +
                    $"of '{operationPart.OperationTypeName}'.");

                return false;
            }

            return true;
        }
    }
}