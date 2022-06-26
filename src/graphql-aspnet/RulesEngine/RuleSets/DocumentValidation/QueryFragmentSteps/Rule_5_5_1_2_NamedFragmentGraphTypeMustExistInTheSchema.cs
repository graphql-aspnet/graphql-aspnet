// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.QueryFragmentSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentPartsNew;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A named fragment's target type must exist within the target schema.
    /// </summary>
    internal class Rule_5_5_1_2_NamedFragmentGraphTypeMustExistInTheSchema
        : DocumentPartValidationRuleStep<INamedFragmentDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var fragment = (INamedFragmentDocumentPart)context.ActivePart;

            if (fragment.GraphType == null)
            {
                this.ValidationError(
                    context,
                    $"Invalid Named Fragment. The fragment named '{fragment.Name}' has a target graph type " +
                    $"of '{fragment.TargetGraphTypeName}' which does not exist on the target schema.  " +
                    $"All named fragments must declare a valid target graph type using the 'on' keyword.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.5.1.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragment-Spread-Type-Existence";

        /*
         *

            // allow inline fragments to not have a target graph type (they inherit their parent's type)
            if (fragment.GraphType == null)
            {
                this.ValidationError(
                    context,
                    "Invalid Fragment. Fragments must declare a target type using the 'on' keyword.");
                return false;
            }*/
    }
}