// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.VariableNodeSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// A rule that dictates that for any set of variables all the names must be unique.
    /// </summary>
    internal class Rule_5_8_1_VariableNamesMustBeUnique
        : DocumentPartValidationRuleStep<IVariableDocumentPart>
    {
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && context.ParentPart is IOperationDocumentPart;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IVariableDocumentPart)context.ActivePart;
            var operation = (IOperationDocumentPart)context.ParentPart;

            var variables = operation.GatherVariables();
            if (variables != null)
            {
                if (variables.TryGetValue(docPart.Name, out var foundVar))
                {
                    if (foundVar != docPart)
                    {
                        this.ValidationError(
                            context,
                            $"Duplicate Variable Name. The variable named '{docPart.Name.ToString()}' must be unique " +
                            "in its contained operation. Ensure that all variable names, per operation, are unique (case-sensitive).");

                        return false;
                    }
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.8.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Variable-Uniqueness";
    }
}