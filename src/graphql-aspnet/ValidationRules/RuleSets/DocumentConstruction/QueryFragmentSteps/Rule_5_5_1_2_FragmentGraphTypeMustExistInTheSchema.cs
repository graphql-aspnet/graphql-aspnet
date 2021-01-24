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
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// An inlined fragmented spread's target type must exist within the target schema.
    /// </summary>
    internal class Rule_5_5_1_2_FragmentGraphTypeMustExistInTheSchema : DocumentConstructionRuleStep<QueryFragment>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var fragment = context.FindContextItem<QueryFragment>();

            // allow inline fragments to not have a target graph type (they inherit their parent's type)
            if (fragment.TargetGraphTypeName == string.Empty)
            {
                this.ValidationError(
                    context,
                    "Invalid Fragment. Fragments must declare a target type using the 'on' keyword.");
                return false;
            }

            if (context.DocumentContext.Schema.KnownTypes.Contains(fragment.TargetGraphTypeName))
                return true;

            this.ValidationError(
                context,
                $"The fragment declares a target type of '{fragment.TargetGraphTypeName}' but no graph type exists " +
                "on the target schema by that name.");

            return false;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.1.2";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Fragment-Spread-Type-Existence";
    }
}