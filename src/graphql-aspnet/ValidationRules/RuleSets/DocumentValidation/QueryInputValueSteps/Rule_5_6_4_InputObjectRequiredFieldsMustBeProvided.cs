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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that any input objects supply all the required fields of their argument definition in the target schema.
    /// </summary>
    internal class Rule_5_6_4_InputObjectRequiredFieldsMustBeProvided : DocumentPartValidationRuleStep
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the input argument if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified input argument; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return context.ActivePart is IInputValueDocumentPart ivdp &&
                ivdp.Value is QueryComplexInputValue;
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
            var graphType = ivdp.GraphType as IInputObjectGraphType;
            var requiredFields = graphType?.Fields.Where(x => x.TypeExpression.IsRequired).ToList();
            var complexValue = ivdp.Value as QueryComplexInputValue;
            if (complexValue == null || requiredFields == null || !requiredFields.Any())
            {
                this.ValidationError(
                    context,
                    ivdp.Node,
                    $"Input type mismatch. The {ivdp.InputType} '{ivdp.Name}' was used like an {TypeKind.INPUT_OBJECT.ToString()} " +
                    $"but contained no fields to evaluate. Check the schema definition for {ivdp.TypeExpression.TypeName}.");
                return false;
            }

            var allFieldsAccountedFor = true;
            foreach (var field in requiredFields)
            {
                if (!complexValue.Arguments.ContainsKey(field.Name))
                {
                    this.ValidationError(
                        context,
                        ivdp.Node,
                        $"The {ivdp.InputType} '{ivdp.Name}' requires a field named '{field.Name}'.");
                    allFieldsAccountedFor = false;
                }
            }

            return allFieldsAccountedFor;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.6.4";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Input-Object-Required-Fields";
    }
}