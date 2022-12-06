// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;

    /// <summary>
    /// <para>(5.5.1.1) Validate that each named fragment has a unique name within the document scope.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/October2021/#sec-Fragment-Name-Uniqueness .</para>
    /// </summary>
    internal class Rule_5_5_1_1_FragmentNamesMustBeUnique
        : DocumentPartValidationRuleStep<INamedFragmentDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var namedFragment = (INamedFragmentDocumentPart)context.ActivePart;

            var metaData = this.GetOrAddMetaData(
                context,
                () => new HashSet<string>());

            // if duplicate named fragments exist an error will be captured
            // when the first of them is processed
            // no need to process the second duplicate again and add another
            // error
            if (metaData.Contains(namedFragment.Name))
                return true;

            metaData.Add(namedFragment.Name);

            if (!context.Document.NamedFragments.IsUnique(namedFragment.Name))
            {
                this.ValidationError(
                    context,
                    $"Duplicate Fragment Name. The fragment name '{namedFragment.Name.ToString()}' must be unique in this document. Ensure that each " +
                    "fragment in the document is unique (case-sensitive).");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.5.1.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragment-Name-Uniqueness";
    }
}