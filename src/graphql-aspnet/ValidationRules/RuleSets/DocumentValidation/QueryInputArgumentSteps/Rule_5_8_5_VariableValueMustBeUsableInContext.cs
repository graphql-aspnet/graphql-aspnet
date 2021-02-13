// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryInputArgumentSteps
{
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// This rule is roughly the same as 5.6.1 (validating a value supplied to a scoped graph type),
    /// but applies to a supplied variable value instead of deconstructed object litteral.
    /// </summary>
    internal class Rule_5_8_5_VariableValueMustBeUsableInContext : DocumentPartValidationRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the input argument if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified input argument; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is QueryInputArgument arg && arg.Value is QueryVariableReferenceInputValue;
        }

        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns>
        ///   <c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            var argument = context.ActivePart as QueryInputArgument;
            var qvr = argument.Value as QueryVariableReferenceInputValue;

            // ensure the type expressions are compatible at the location used
            if (!qvr.Variable.TypeExpression.Equals(argument.TypeExpression))
            {
                this.ValidationError(
                    context,
                    argument.Node,
                    "Invalid Variable Argument. The type expression for the variable used on the " +
                    $"{argument.InputType} '{argument.Name}' could " +
                    $"not be successfully coerced to the required type. Expected '{argument.TypeExpression}' but got '{qvr.Variable.TypeExpression}'. Double check " +
                    $"the declared graph type of the variable and ensure it matches the required type of '{argument.Name}'.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.8.5";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-All-Variable-Usages-are-Allowed";
    }
}