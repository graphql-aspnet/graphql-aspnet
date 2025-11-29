// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>(5.2.4.1) A rule to validate that a subscription operation has 1 and only 1 root level field declaration.</para>
    /// <para>Reference: <see href="https://spec.graphql.org/September2025/#sec-Single-Root-Field" /> .</para>
    /// </summary>
    internal class Rule_5_2_4_1_SubscriptionsRequire1RootField
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
                    operation.SourceLocation,
                    $"Invalid Subscription. Expected exactly 1 root child field, " +
                    $"recieved {fieldCollection?.Children.Count ?? 0} child fields.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.2.4.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Single-Root-Field";
    }
}