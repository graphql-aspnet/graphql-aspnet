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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.FieldResolution;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Middleware.FieldExecution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.ValidationRules.RuleSets.FieldResolution.Common;

    /// <summary>
    /// A rule that inspects a completed result to ensure it conforms to the field's type expression
    /// requirements for general existence.
    /// </summary>
    internal class Rule_6_4_3_ValueCompletion : FieldResolutionRuleStep
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
            var dataObject = context.ResultData;

            // did they forget to await a task and accidentally returned Task<T> from their resolver?
            // put in a special error message fro better feed back
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

            var dataItemTypeExpression = context.DataItem.TypeExpression;
            if (dataItemTypeExpression.IsRequired && dataObject == null)
            {
                // 6.4.3 section 1c
                this.ValidationError(
                context,
                $"Field '{context.FieldPath}' expected a non-null result but received {{null}}.");

                context.DataItem.InvalidateResult();
            }

            // 6.4.3 section 2
            if (dataObject == null)
                return true;

            // 6.4.3 section 3, ensure list type in the result object for a type expression that is a list.
            // This is a quick short cut and customed error message for a common top-level list mismatch.
            if (dataItemTypeExpression.IsListOfItems && !GraphValidation.IsValidListType(dataObject.GetType()))
            {
                this.ValidationError(
                    context,
                    $"Field '{context.FieldPath}' was expected to return a list of items but instead returned a single item.");

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

            var rootSourceType = GraphValidation.EliminateWrappersFromCoreType(dataObject.GetType());
            var sourceTypeMatches = true;

            // virtual graph types aren't real, they have no real concrete type
            // and can be safely skipped
            if (!expectedGraphType.IsVirtual)
            {
                // Check that the actual .NET type of the result data IS (or can be cast to) the expected .NET
                // type for the graphtype of the field being checked
                var expectedSourceType = context.Schema.KnownTypes.FindConcreteType(expectedGraphType);
                sourceTypeMatches = Validation.IsCastable(rootSourceType, expectedSourceType);
                if (!sourceTypeMatches)
                {
                    this.ValidationError(
                        context,
                        $"A field resolver for '{context.Field.Route.Path}' generated a result " +
                        "object type not known to the target schema. See exception for " +
                        "details",
                        new GraphExecutionException(
                            $"The class '{rootSourceType.FriendlyName()}' does not inherit from '{expectedSourceType.FriendlyName()}' " +
                            $"as expected by the target schema. It cannot be used to resolve the field '{context.Field.Route.Path}'."));

                    context.DataItem.InvalidateResult();
                }
            }

            // Perform a deep check of the meta-type chain (list and nullability wrappers) against the result data.
            // For example, if the type expression is [[SomeType]] ensure the result object is List<List<T>> etc.)
            // however, use the type name of the actual data object, not the graph type itself
            // we only want to check the type expression wrappers in this step
            var mangledTypeExpression = dataItemTypeExpression.CloneTo(rootSourceType.Name);
            if (!mangledTypeExpression.Matches(dataObject))
            {
                // generate a valid, properly cased type expression reference for the data that was provided
                var actualExpression = GraphValidation.GenerateTypeExpression(context.ResultData.GetType());

                // if the .NET Type check was considered valid, don't pass a wrong type name down
                // on the error messages, use the correct type of the field. (i.e. use 'Donut' not 'DonutProxy')
                if (sourceTypeMatches)
                    actualExpression = actualExpression.CloneTo(expectedGraphType.Name);

                // 6.4.3  section 4 & 5
                this.ValidationError(
                    context,
                    $"The resolved value for field '{context.FieldPath}' does not match the required type expression. " +
                    $"Expected {dataItemTypeExpression} but got {actualExpression}.");

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