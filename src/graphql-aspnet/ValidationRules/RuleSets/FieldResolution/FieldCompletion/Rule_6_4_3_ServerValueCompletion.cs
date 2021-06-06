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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// A rule that inspects only a root object (not a list of objects) to ensure that the actual .NET class type
    /// of the item is allowed for the graph type of the field being resolved.
    /// </summary>
    internal class Rule_6_4_3_ServerValueCompletion : FieldResolutionRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the TContext
        /// if it cannot process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(FieldValidationContext context)
        {
            return context?.DataItem != null
                && context.DataItem.Status == FieldItemResolutionStatus.ResultAssigned
                && context.ResultData != null
                && !context.DataItem.TypeExpression.IsListOfItems;
        }

        /// <summary>
        /// Validates the completed field context to ensure it is "correct" against the specification before finalizing its reslts.
        /// </summary>
        /// <param name="context">The context containing the resolved field.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(FieldValidationContext context)
        {
            var dataObject = context.ResultData;
            var dataItemTypeExpression = context.DataItem.TypeExpression;

            // did they forget to await a task and accidentally returned Task<T> from their resolver?
            // put in a special error message for better feed back
            if (dataObject is Task)
            {
                this.ValidationError(
                    context,
                    $"A field resolver for '{context.FieldPath}' yielded an invalid data object. See exception " +
                    "for details. ",
                    new GraphExecutionException(
                        $"The field '{context.FieldPath}' yielded a {nameof(Task)} as its result but expected a value. " +
                        "Did you forget to await an async method?"));

                context.DataItem.InvalidateResult();
                return true;
            }

            var expectedGraphType = context.Schema?.KnownTypes.FindGraphType(context.Field);
            if (expectedGraphType == null)
            {
                this.ValidationError(
                    context,
                    $"The graph type for field '{context.FieldPath}' ({dataItemTypeExpression?.TypeName}) does not exist on the target schema. The field" +
                    "cannot be properly evaluated.");

                context.DataItem.InvalidateResult();
                return true;
            }

            // virtual graph types aren't real, they can be safely skipped
            if (expectedGraphType.IsVirtual)
                return true;

            var rootSourceType = GraphValidation.EliminateWrappersFromCoreType(dataObject.GetType());

            // Check that the actual .NET type of the result data IS (or can be cast to) the expected .NET
            // type for the graphtype of the field being checked
            var analysisResult = context.Schema.KnownTypes.AnalyzeRuntimeConcreteType(expectedGraphType, rootSourceType);

            if (!analysisResult.ExactMatchFound)
            {
                string foundTypeNames;
                if (!analysisResult.FoundTypes.Any())
                    foundTypeNames = "~none~";
                else
                    foundTypeNames = string.Join(", ", analysisResult.FoundTypes.Select(x => x.FriendlyName()));

                this.ValidationError(
                    context,
                    $"A field resolver for '{context.Field.Route.Path}' generated a result " +
                    "object type not known to the target schema. See exception for " +
                    "details",
                    new GraphExecutionException(
                        $"For target field of '{context.FieldPath}' (Graph Type: {expectedGraphType.Name}, Kind: {expectedGraphType.Kind}), a supplied object " +
                        $"of class '{rootSourceType.FriendlyName()}' attempted to fill the request but graphql was not able to determine which " +
                        $"of the matched concrete types to use and cannot resolve the field. Matched Types: [{foundTypeNames}]"));

                context.DataItem.InvalidateResult();
            }
            else
            {
                // once the type is confirmed, confirm the actual value
                // for scenarios such as where a number may be masqurading as an enum but the enum doesn't define
                // a label for said number.
                var isValidValue = expectedGraphType.ValidateObject(dataObject);
                if (!isValidValue)
                {
                    string actual = string.Empty;
                    if (expectedGraphType is ObjectGraphType)
                        actual = dataObject.GetType().Name;
                    else
                        actual = dataObject.ToString();

                    this.ValidationError(
                        context,
                        $"A resolved value for field '{context.FieldPath}' does not match the required graph type. " +
                        $"Expected '{expectedGraphType.Name}' but got '{actual}'.");

                    context.DataItem.InvalidateResult();
                }
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