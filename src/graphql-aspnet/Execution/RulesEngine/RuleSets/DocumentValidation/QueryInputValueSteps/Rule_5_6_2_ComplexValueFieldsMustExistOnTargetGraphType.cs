// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryInputValueSteps
{
    using System.Linq;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Ensures that any field parsed from an object literal actual exists on the INPUT_OBJECT and that no extra fields are
    /// provided.
    /// </summary>
    internal class Rule_5_6_2_ComplexValueFieldsMustExistOnTargetGraphType
        : DocumentPartValidationRuleStep<IComplexSuppliedValueDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context) && ((IComplexSuppliedValueDocumentPart)context.ActivePart).GraphType != null;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IComplexSuppliedValueDocumentPart)context.ActivePart;
            var targetGraphType = docPart.GraphType as IInputObjectGraphType;

            // ensure that the graph type assigned to the complex part is an INPUT_OBJECT
            if (targetGraphType == null)
            {
                this.ValidationError(
                 context,
                 $"The {docPart.GraphType.Kind} type '{docPart.GraphType.Name}' cannot contain any fields.");

                return false;
            }

            // ensures that the field exists in the graphtype assigned to the complex part
            var isSuccessful = true;
            foreach (var field in docPart.Children.OfType<IInputObjectFieldDocumentPart>())
            {
                if (!targetGraphType.Fields.ContainsKey(field.Name))
                {
                    this.ValidationError(
                        context,
                        $"The {targetGraphType.Kind} type '{targetGraphType.Name}' does not " +
                        $"define a field named '{field.Name}'.");

                    isSuccessful = false;
                }
            }

            return isSuccessful;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.6.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Input-Object-Field-Names";
    }
}