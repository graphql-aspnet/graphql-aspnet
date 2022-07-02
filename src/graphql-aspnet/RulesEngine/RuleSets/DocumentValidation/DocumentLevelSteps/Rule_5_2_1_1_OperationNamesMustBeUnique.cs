// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.DocumentLevelSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// <para>(5.2.1.1) Validate that each top level operation has a unique name within the document scope.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/October2021/#sec-Operation-Name-Uniqueness" .</para>
    /// </summary>
    internal class Rule_5_2_1_1_OperationNamesMustBeUnique
        : DocumentPartValidationRuleStep<IGraphQueryDocument>
    {
        /// <inheritdoc/>
        public override bool Execute(DocumentValidationContext context)
        {
            var document = context.ActivePart as IGraphQueryDocument;

            var allNamesUnique = true;
            var names = new HashSet<string>();
            foreach (var operation in document.Operations)
            {
                var operationName = operation.Name.ToString();
                if (!string.IsNullOrWhiteSpace(operationName))
                {
                    if (names.Contains(operationName))
                    {
                        this.ValidationError(
                            context,
                            operation.Node,
                            $"Duplicate Operation Name. The operation named '{operationName}' must be unique " +
                            "in this document. Ensure that each query has a unique name (case-sensitive).");

                        allNamesUnique = false;
                    }

                    names.Add(operationName);
                }
            }

            return allNamesUnique;
        }

        /// <inheritdoc/>
        public override string RuleNumber => "5.2.1.1";

        /// <inheritdoc/>
        protected override string RuleAnchorTag => "#sec-Operation-Name-Uniqueness";
    }
}