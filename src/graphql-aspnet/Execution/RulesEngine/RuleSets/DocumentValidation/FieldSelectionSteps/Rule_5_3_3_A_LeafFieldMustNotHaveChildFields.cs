// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSteps
{
    using System.Linq;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>(5.3.3)A rule that dictates for any given graph type returned by a field if that graph type
    /// is a leaf type then the field cannot declare a selection set of fields to return.</para>
    /// <para>Reference: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-Leaf-Field-Selections" /> .</para>
    /// </summary>
    internal class Rule_5_3_3_A_LeafFieldMustNotHaveChildFields
        : DocumentPartValidationRuleStep<IFieldDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var field = (IFieldDocumentPart)context.ActivePart;
            var selectionSet = field.FieldSelectionSet;
            var hasChildFields = selectionSet != null && selectionSet.ExecutableFields.Any();

            if (field.GraphType.Kind.IsLeafKind() && hasChildFields)
            {
                this.ValidationError(
                    context,
                    $"The graph type '{field.GraphType.Name}' is of kind '{field.GraphType.Kind}'. A returned fieldset cannot be declared " +
                    "against it.");

                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.3.3";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Leaf-Field-Selections";
    }
}