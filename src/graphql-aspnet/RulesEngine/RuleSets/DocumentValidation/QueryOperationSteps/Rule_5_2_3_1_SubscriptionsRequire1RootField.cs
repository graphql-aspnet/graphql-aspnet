// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.OperationNodeSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule to validate that a subscription operation has 1 and only 1 root level field declaration.
    /// </summary>
    internal class Rule_5_2_3_1_SubscriptionsRequire1RootField
        : DocumentPartValidationRuleStep<IOperationDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && ((IOperationDocumentPart)context.ActivePart).OperationType == GraphOperationType.Subscription;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var operation = (IOperationDocumentPart)context.ActivePart;

            var fieldCollection = operation.FieldSelectionSet;
            if (fieldCollection == null || fieldCollection.Children.Count != 1)
            {
                this.ValidationError(
                    context,
                    operation.Node,
                    $"Invalid Subscription. Expected exactly 1 root child field, " +
                    $"recieved {fieldCollection?.Children.Count ?? 0} child fields.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.2.3.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Single-root-field";
    }
}