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
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that the value of an input argument passed on the query document can be converted into the type required by the argument definition on the schema.
    /// </summary>
    internal class Rule_5_6_1_ValueMustBeCoerceable : DocumentPartValidationRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the input argument if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified input argument; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is IInputValueDocumentPart ivdp && !(ivdp.Value is QueryVariableReferenceInputValue);
        }

        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            var ivdp = context.ActivePart as IInputValueDocumentPart;
            var value = ivdp.Value;

            // variables do not have to supply a default value
            if (value == null && ivdp is QueryVariable)
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
        private bool EvaluateContextData(QueryInputValue value)
        {
            if (value == null)
                return false;

            var queryArg = value.OwnerArgument;
            var valueTypeExpression = value.OwnerArgument.TypeExpression.Clone();
            var valueSet = new List<QueryInputValue>();
            valueSet.Add(value);

            // walk all type expression wrappers (IsList and IsNotNull)
            // and check the items in scope at the given level for their "correctness"
            // when a list is encountered expand its items so as to check the "next level"
            // such as when a type expression is a list of lists (e.g. [[Int]])
            while (valueTypeExpression.Wrappers.Any())
            {
                var nextValueSet = new List<QueryInputValue>();
                foreach (var item in valueSet)
                {
                    // ensure a variable reference is of the correct type expression
                    // for this level
                    // then elimnate it
                    if (item is QueryVariableReferenceInputValue qvr)
                    {
                        var typeExpression = qvr.Variable.TypeExpression;
                        if (!typeExpression.Equals(valueTypeExpression))
                            return false;

                        continue;
                    }

                    switch (valueTypeExpression.Wrappers[0])
                    {
                        case MetaGraphTypes.IsNotNull:
                            if (item is QueryNullInputValue)
                                return false;
                            else
                                nextValueSet.Add(item);

                            break;

                        case MetaGraphTypes.IsList:
                            if (!(item is QueryListInputValue qliv))
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
                if (valueToEvaluate is QueryNullInputValue)
                    continue;

                switch (queryArg.GraphType.Kind)
                {
                    case TypeKind.SCALAR:
                        var scalarType = queryArg.GraphType as IScalarGraphType;
                        var scalarValue = valueToEvaluate as QueryScalarInputValue;
                        if (scalarType == null ||
                            scalarValue == null ||
                            !scalarType.ValueType.HasFlag(scalarValue.ValueType))
                        {
                            return false;
                        }

                        break;

                    case TypeKind.ENUM:
                        var enumType = queryArg.GraphType as IEnumGraphType;
                        var enumValue = valueToEvaluate as QueryEnumInputValue;

                        if (enumType == null || enumValue == null)
                            return false;

                        if (!enumType.Values.ContainsKey(enumValue.Value.ToString()))
                            return false;

                        break;

                    case TypeKind.INPUT_OBJECT:
                        var inputGraphType = queryArg.GraphType as IInputObjectGraphType;
                        var complexValue = valueToEvaluate as QueryComplexInputValue;

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
        private bool EnsureSingleValueChain(QueryInputValue value)
        {
            var valueToCheck = value.OwnerValue;
            while (valueToCheck != null)
            {
                if (valueToCheck is QueryListInputValue qliv && qliv.ListItems.Count > 1)
                    return false;

                valueToCheck = valueToCheck.OwnerValue;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.6.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Values-of-Correct-Type";
    }
}