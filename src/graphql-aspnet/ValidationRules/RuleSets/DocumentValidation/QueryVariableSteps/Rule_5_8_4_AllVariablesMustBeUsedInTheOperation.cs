// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryVariableSteps
{
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Ensures that any declared variable was referenced in the operation.
    /// </summary>
    internal class Rule_5_8_4_AllVariablesMustBeUsedInTheOperation : DocumentPartValidationRuleStep<QueryVariable>
    {
        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            var variable = (QueryVariable)context.ActivePart;
            if (!variable.IsReferenced)
            {
                this.ValidationError(
                    context,
                    variable.Node,
                    $"The variable '{variable.Name}' was not used within the target operation. " +
                    "All declared variables must be used at least once.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.8.4";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-All-Variables-Used";
    }
}