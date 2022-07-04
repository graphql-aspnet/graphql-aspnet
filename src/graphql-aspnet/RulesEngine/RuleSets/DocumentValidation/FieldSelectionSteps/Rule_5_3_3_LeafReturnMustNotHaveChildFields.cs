// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.FieldSelectionSteps
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// <para>(5.3.3)A rule that dictates for any given graph type returned by a field if that graph type
    /// is a leaf type then the field cannot declare a selection set of fields to return.</para>
    /// <para>Reference: https://graphql.github.io/graphql-spec/June2018/#sec-Leaf-Field-Selections .</para>
    /// </summary>
    internal class Rule_5_3_3_LeafReturnMustNotHaveChildFields
        : DocumentPartValidationRuleStep<IFieldDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context) && ((IFieldDocumentPart)context.ActivePart)
                       .Name
                       .Span
                       .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var field = (IFieldDocumentPart)context.ActivePart;
            var selectionSet = field.FieldSelectionSet;
            var hasChildFields = selectionSet != null && selectionSet.ExecutableFields.Count > 0;

            if (field.GraphType.Kind.IsLeafKind() && hasChildFields)
            {
                this.ValidationError(
                    context,
                    $"The graph type '{field.GraphType.Name}' is of kind '{field.GraphType.Kind.ToString()}'. It cannot declare a fieldset " +
                    "to return.");

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