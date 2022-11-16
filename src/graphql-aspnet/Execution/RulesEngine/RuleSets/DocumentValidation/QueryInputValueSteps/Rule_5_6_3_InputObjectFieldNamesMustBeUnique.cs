// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Ensures that there are no duplicate fields provided on a complex input object.
    /// </summary>
    internal class Rule_5_6_3_InputObjectFieldNamesMustBeUnique
        : DocumentPartValidationRuleStep<IComplexSuppliedValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var complexValue = (IComplexSuppliedValueDocumentPart)context.ActivePart;
            var ownerArgument = context.ParentPart as IInputArgumentDocumentPart;

            var fieldNames = new HashSet<string>();
            var failedFields = new HashSet<string>();
            foreach (var field in complexValue.Children.OfType<IInputObjectFieldDocumentPart>())
            {
                if (fieldNames.Contains(field.Name))
                {
                    if (!failedFields.Contains(field.Name))
                    {
                        this.ValidationError(
                            context,
                            $"Fields on input objects must be unique. The supplied value for argument '{ownerArgument?.Name}' " +
                            $"defines '{field.Name}' more than once.");

                        failedFields.Add(field.Name);
                    }
                }

                fieldNames.Add(field.Name);
            }

            return failedFields.Count == 0;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.6.3";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Input-Object-Field-Uniqueness";
    }
}