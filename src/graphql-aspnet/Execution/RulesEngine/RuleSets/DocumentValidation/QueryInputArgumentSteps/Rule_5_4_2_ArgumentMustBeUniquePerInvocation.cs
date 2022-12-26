// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryInputArgumentSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// A rule that ensures that for any given input node, the name of the node is unique among the defined set of inputs
    /// on the target field in the user's query document.
    /// </summary>
    internal class Rule_5_4_2_ArgumentMustBeUniquePerInvocation
        : DocumentPartValidationRuleStep<IInputArgumentDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IInputArgumentDocumentPart)context.ActivePart;

            var metadata = this.GetOrAddMetaData(
                context,
                () => new Dictionary<IDocumentPart, HashSet<string>>());

            // short cut. if an arugment IS declared more than once
            // this rule would be run twice but catching the duplication
            // once is enough to fail validation, we can the other
            // checks
            HashSet<string> checkedArguments;
            if (metadata.ContainsKey(docPart.Parent))
            {
                checkedArguments = metadata[docPart.Parent];
            }
            else
            {
                checkedArguments = new HashSet<string>();
                metadata.Add(docPart.Parent, checkedArguments);
            }

            if (checkedArguments.Contains(docPart.Name))
                return true;

            checkedArguments.Add(docPart.Name);

            if (context.ParentPart is IInputArgumentCollectionContainer iac
                && !iac.Arguments.IsUnique(docPart.Name))
            {
                var parentType = "unknown type";
                var parentName = "~unknown~";

                if (context.ParentPart is IDirectiveDocumentPart ddp)
                {
                    parentType = "directive";
                    parentName = ddp.DirectiveName;
                }
                else if (context.ParentPart is IFieldDocumentPart fdp)
                {
                    parentType = "field";
                    parentName = fdp.Name.ToString();
                }

                this.ValidationError(
                    context,
                    $"The {parentType} '{parentName}' already contains an input argument named '{docPart.Name}'. Input arguments " +
                    $"must be unique per {parentType} invocation.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.4.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Argument-Uniqueness";
    }
}