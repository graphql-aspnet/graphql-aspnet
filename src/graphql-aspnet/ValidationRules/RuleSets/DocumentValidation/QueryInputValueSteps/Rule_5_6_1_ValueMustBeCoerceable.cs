// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;

    /// <summary>
    /// Ensures that the value of an input argument passed on the query document can be converted into the type required by the argument definition on the schema.
    /// </summary>
    internal class Rule_5_6_1_ValueMustBeCoerceable : DocumentPartValidationRuleStep
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is IAssignableValueDocumentPart ivdp && !(ivdp.Value is DocumentVariableReferenceInputValue);
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var ivdp = context.ActivePart as IAssignableValueDocumentPart;
            var value = ivdp.Value;

            // variables do not have to supply a default value
            if (value == null && ivdp is DocumentVariable)
                return true;

            if (!this.EvaluateContextData(value))
            {
                this.ValidationError(
                    context,
                    value.ValueNode,
                    $"Invalid {ivdp.InputType}. The value for the input item named '{ivdp.Name}' could " +
                    $"not be successfully coerced to the required type of '{ivdp.TypeExpression}'.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the context input data for correctness against the argument definition on the schema.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the data is valid for this rule, <c>false</c> otherwise.</returns>
        private bool EvaluateContextData(ISuppliedValueDocumentPart value)
        {
            if (value == null)
                return false;

            var queryArg = value.Owner;
            var valueTypeExpression = value.Owner.TypeExpression.Clone();
            var valueSet = new List<ISuppliedValueDocumentPart>();
            valueSet.Add(value);

            // walk all type expression wrappers (IsList and IsNotNull)
            // and check the items in scope at the given level for their "correctness"
            // when a list is encountered expand its items so as to check the "next level"
            // such as when a type expression is a list of lists (e.g. [[Int]])
            while (valueTypeExpression.Wrappers.Any())
            {
                var nextValueSet = new List<ISuppliedValueDocumentPart>();
                foreach (var item in valueSet)
                {
                    // ensure a variable reference is of the correct type expression
                    // for this level
                    // then elimnate it
                    if (item is DocumentVariableReferenceInputValue qvr)
                    {
                        var typeExpression = qvr.Variable.TypeExpression;
                        if (!typeExpression.Equals(valueTypeExpression))
                            return false;

                        continue;
                    }

                    switch (valueTypeExpression.Wrappers[0])
                    {
                        case MetaGraphTypes.IsNotNull:
                            if (item is DocumentNullSuppliedValue)
                                return false;
                            else
                                nextValueSet.Add(item);

                            break;

                        case MetaGraphTypes.IsList:
                            if (!(item is DocumentListSuppliedValue qliv))
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
                if (valueToEvaluate is DocumentNullSuppliedValue)
                    continue;

                switch (queryArg.GraphType.Kind)
                {
                    case TypeKind.SCALAR:
                        var scalarType = queryArg.GraphType as IScalarGraphType;
                        var scalarValue = valueToEvaluate as DocumentScalarSuppliedValue;
                        if (scalarType == null ||
                            scalarValue == null ||
                            !scalarType.ValueType.HasFlag(scalarValue.ValueType))
                        {
                            return false;
                        }

                        break;

                    case TypeKind.ENUM:
                        var enumType = queryArg.GraphType as IEnumGraphType;
                        var enumValue = valueToEvaluate as DocumentEnumSuppliedValue;

                        if (enumType == null || enumValue == null)
                            return false;

                        if (!enumType.Values.ContainsKey(enumValue.Value.ToString()))
                            return false;

                        break;

                    case TypeKind.INPUT_OBJECT:
                        var inputGraphType = queryArg.GraphType as IInputObjectGraphType;
                        var complexValue = valueToEvaluate as DocumentComplexSuppliedValue;

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
        /// From the curent context inspect every "up level" parent context to ensure that none were lists
        /// or if they were, that the list contained at most one item.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the context chain represents one single value, <c>false</c> otherwise.</returns>
        private bool EnsureSingleValueChain(ISuppliedValueDocumentPart value)
        {
            var valueToCheck = value.ParentValue;
            while (valueToCheck != null)
            {
                if (valueToCheck is DocumentListSuppliedValue qliv && qliv.ListItems.Count > 1)
                    return false;

                valueToCheck = valueToCheck.ParentValue;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.6.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Values-of-Correct-Type";
    }
}