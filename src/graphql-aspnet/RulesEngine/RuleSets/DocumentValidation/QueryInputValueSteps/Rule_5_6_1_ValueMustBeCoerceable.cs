// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that the value of an input argument passed on the query document
    /// can be converted into the type required by the argument definition on the schema.
    /// </summary>
    internal class Rule_5_6_1_ValueMustBeCoerceable : DocumentPartValidationRuleStep<IInputValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                      && !(((IInputValueDocumentPart)context.ActivePart).Value is IVariableUsageDocumentPart)
                      && ((IInputValueDocumentPart)context.ActivePart).TypeExpression != null;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var inputValue = context.ActivePart as IInputValueDocumentPart;

            if (!this.EvaluateInputValue(inputValue))
            {
                this.ValidationError(
                    context,
                    inputValue.Value?.Node ?? inputValue.Node,
                    $"Invalid input value. The value for the input item named '{inputValue.Name}' could " +
                    $"not be coerced to the required type of '{inputValue.TypeExpression}'.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the input value for correctness against the argument's type expression.
        /// </summary>
        /// <param name="queryValue">The query argument, with an supplied value, to evaluate.</param>
        /// <returns><c>true</c> if the data is valid for this rule, <c>false</c> otherwise.</returns>
        private bool EvaluateInputValue(IInputValueDocumentPart queryValue)
        {
            if (queryValue?.TypeExpression == null || queryValue.Value == null)
                return false;

            var argumentValue = queryValue.Value;
            var valueTypeExpression = queryValue.TypeExpression.Clone();
            var valueSet = new List<ISuppliedValueDocumentPart>();
            valueSet.Add(argumentValue);

            // walk all type expression wrappers (IsList and IsNotNull)
            // and check the items in scope at the given level for their "correctness"
            // when a list is encountered expand its items so as to check the "next level"
            // such as when a type expression is a list of lists (e.g. [[Int]])
            while (valueTypeExpression.Wrappers.Any())
            {
                var nextValueSet = new List<ISuppliedValueDocumentPart>();
                foreach (var item in valueSet)
                {
                    // variable usage expression are evaluated as part of
                    // 5.8.5.
                    if (item is IVariableUsageDocumentPart)
                        continue;

                    switch (valueTypeExpression.Wrappers[0])
                    {
                        case MetaGraphTypes.IsNotNull:
                            if (item is INullSuppliedValueDocumentPart)
                                return false;
                            else
                                nextValueSet.Add(item);

                            break;

                        case MetaGraphTypes.IsList:
                            if (!(item is IListSuppliedValueDocumentPart qliv))
                            {
                                if (!this.EnsureSingleValueChain(item))
                                    return false;

                                nextValueSet.Add(item);
                            }
                            else
                            {
                                foreach (var childItem in qliv.ListItems)
                                    nextValueSet.Add(childItem);
                            }

                            break;
                    }
                }

                valueSet = nextValueSet;
                valueTypeExpression = valueTypeExpression.UnWrapExpression();
            }

            // at this point, the object set has been "expression checked" and the value (and children)
            // is garunteed to be reduced to a set of core graph types. Evaluate that the values are of the correct core graph type.
            // Null values are valid for all types at this stage
            foreach (var valueToEvaluate in valueSet)
            {
                if (valueToEvaluate is INullSuppliedValueDocumentPart)
                    continue;

                switch (queryValue.GraphType.Kind)
                {
                    case TypeKind.SCALAR:
                        var scalarType = queryValue.GraphType as IScalarGraphType;
                        var scalarValue = valueToEvaluate as IScalarSuppliedValue;
                        if (scalarType == null ||
                            scalarValue == null ||
                            !scalarType.ValueType.HasFlag(scalarValue.ValueType))
                        {
                            return false;
                        }

                        break;

                    case TypeKind.ENUM:
                        var enumType = queryValue.GraphType as IEnumGraphType;
                        var enumValue = valueToEvaluate as IEnumSuppliedValueDocumentPart;

                        if (enumType == null || enumValue == null)
                            return false;

                        if (!enumType.Values.ContainsKey(enumValue.Value.ToString()))
                            return false;

                        break;

                    case TypeKind.INPUT_OBJECT:
                        var inputGraphType = queryValue.GraphType as IInputObjectGraphType;
                        var complexValue = valueToEvaluate as IComplexSuppliedValueDocumentPart;

                        if (inputGraphType == null || complexValue == null)
                            return false;

                        break;

                    default: // graph type is not valid for input arguments, just fail
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// From the curent document part inspect every "up level" item to ensure that none were lists
        /// or if they were, that the list contained at most one item.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>
        ///   <c>true</c> if the parental chain represents one single value, <c>false</c> otherwise.</returns>
        private bool EnsureSingleValueChain(ISuppliedValueDocumentPart value)
        {
            var valueToCheck = value.Parent;
            while (valueToCheck != null)
            {
                if (!(valueToCheck is ISuppliedValueDocumentPart))
                    break;

                if (valueToCheck is IListSuppliedValueDocumentPart qliv && qliv.ListItems.Count > 1)
                    return false;

                valueToCheck = valueToCheck.Parent;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.6.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Values-of-Correct-Type";
    }
}