// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.FieldCompletion
{
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// A rule that inspects a completed result to ensure it conforms to the field's type expression
    /// requirements for general existence.
    /// </summary>
    internal class Rule_6_4_3_SchemaValueCompletion : FieldResolutionRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the TContext
        /// if it cannot process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(FieldValidationContext context)
        {
            return context?.DataItem != null && context.DataItem.Status == FieldItemResolutionStatus.ResultAssigned;
        }

        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(FieldValidationContext context)
        {
            var typeExpression = context.TypeExpression;
            if (typeExpression.IsRequired && context.ResultData == null)
            {
                // 6.4.3 section 1c
                this.ValidationError(
                context,
                $"Field '{context.FieldPath}' expected a non-null result but received {{null}}.");

                context.DataItem.InvalidateResult();
            }

            // 6.4.3 section 2
            if (context.ResultData == null)
                return true;

            // 6.4.3 section 3, ensure an IEnumerable for a type expression that is a list
            if (typeExpression.IsListOfItems && !GraphValidation.IsValidListType(context.ResultData.GetType()))
            {
                this.ValidationError(
                    context,
                    $"Field '{context.FieldPath}' was expected to return a list of items but instead returned a single item.");

                context.DataItem.InvalidateResult();
                return true;
            }

            var graphType = context.Schema?.KnownTypes.FindGraphType(typeExpression?.TypeName);
            if (graphType == null)
            {
                this.ValidationError(
                    context,
                    $"The graph type for field '{context.FieldPath}' ({typeExpression?.TypeName}) does not exist on the target schema. The field" +
                    "cannot be properly evaluated.");

                context.DataItem.InvalidateResult();
            }
            else if (!typeExpression.Matches(context.ResultData, graphType.ValidateObject))
            {
                // generate a valid, properly cased type expression reference for the data that was provided
                var actualExpression = GraphValidation.GenerateTypeExpression(context.ResultData.GetType());
                var coreType = GraphValidation.EliminateWrappersFromCoreType(context.ResultData.GetType());
                var actualType = context.Schema.KnownTypes.FindGraphType(coreType);
                if (actualType != null)
                    actualExpression = actualExpression.CloneTo(actualType.Name);

                // 6.4.3  section 4 & 5
                this.ValidationError(
                    context,
                    $"The resolved value for field '{context.FieldPath}' does not match the required type expression. " +
                    $"Expected {typeExpression} but got {actualExpression}.");

                context.DataItem.InvalidateResult();
            }

            return true;
        }

        /// <summary>
        /// Determines where the context is in a state such that it should continue processing its children, if any exist.
        /// Returning false will cease processing child items under the active item of this context. This can be useful
        /// if/when a situation in a parent disqualifies all other items in a processing tree. This step is always executed
        /// even if the primary execution is skipped or fails.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns><c>true</c> if child rulesets should be executed, <c>false</c> otherwise.</returns>
        public override bool ShouldAllowChildContextsToExecute(FieldValidationContext context)
        {
            // if a context indicates an error when validating schema values, downstream children don't
            // matter. useful in preventing individual list item checks when the list itself
            // doesnt conform to required values.
            return context?.DataItem != null && !context.DataItem.Status.IndicatesAnError();
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "6.4.3";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Value-Completion";
    }
}