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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that for the active <see cref="IFieldDocumentPart"/> that it exists on the currently
    /// scoped graph type.
    /// </summary>
    internal class Rule_5_3_1_FieldMustExistOnTargetGraphType
        : DocumentPartValidationRuleStep<IFieldDocumentPart>
    {
        /// <summary>
        /// Creates a common message to indicate and invalid or missing field.
        /// </summary>
        /// <param name="graphType">The graph type expected to contain the field.</param>
        /// <param name="fieldName">Name of the non-existent field.</param>
        /// <returns>System.String.</returns>
        public static string InvalidFieldMessage(string graphType, string fieldName)
        {
            return $"The graph type '{graphType}' does not contain a field named '{fieldName}'.";
        }

        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            if (!base.ShouldExecute(context))
                return false;

            return ((IFieldDocumentPart)context.ActivePart).Name != Constants.ReservedNames.TYPENAME_FIELD;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IFieldDocumentPart)context.ActivePart;
            var selectionSetGraphType = docPart.Parent.GraphType;
            var fieldContainer = selectionSetGraphType as IGraphFieldContainer;

            // validate that the field is part of a selection that that belongs to a graph
            // type that has fields
            //
            // Design Note: Rule 5.3.1 allows for unions because of the '__typename' field/
            //              However, this rule instance does not validate against the speical '__typename '
            //              field so there is never a case where this rule encounters a field declared against
            //              a union is valid
            if (selectionSetGraphType == null
                || (selectionSetGraphType.Kind != TypeKind.OBJECT
                    && selectionSetGraphType.Kind != TypeKind.INTERFACE))
            {
                if (selectionSetGraphType != null && selectionSetGraphType.Kind == TypeKind.UNION)
                {
                    // special error message for union types
                    this.ValidationError(
                        context,
                        $"The field '{docPart.Name.ToString()}' cannot be directly selected from type '{selectionSetGraphType.Name}'. " +
                        $"Fields cannot be directly selected from {nameof(TypeKind.UNION)} type selection sets.");
                }
                else if (selectionSetGraphType != null)
                {
                    this.ValidationError(
                        context,
                        InvalidFieldMessage(selectionSetGraphType.Name, docPart.Name.ToString()));
                }
                else
                {
                    // should be impossible given field selection set validation
                    this.ValidationError(
                        context,
                        $"The document field '{docPart.Name.ToString()}' cannot be validated. Its parent selection set " +
                        $"does not declare a graph type to validate against.");
                }

                return false;
            }

            // ensure a field reference (and its resultant graph type) was properly mapped
            // during construction and that the field belongs to the graph type in scope.
            if (docPart.GraphType == null || docPart.Field == null || !fieldContainer.Fields.ContainsKey(docPart.Field.Name))
            {
                this.ValidationError(
                    context,
                    InvalidFieldMessage(selectionSetGraphType.Name, docPart.Name.ToString()));

                return false;
            }
            else if (docPart.Field.Name != docPart.Name.ToString())
            {
                // ensure that the field reference assigned to fulfill the request matches the same name
                // as that requested in the query document. Its possible for a directive execution to
                // change out the field reference to a different field on the same parent type. This is not allowed.
                this.ValidationError(
                        context,
                        $"Invalid field reference. The field name requested '{docPart.Name.ToString()}' does not match the field " +
                        $"assigned to fulfill the request '{docPart.Field.Name}'");
            }
            else
            {
                // ensure that the field reference is to the field on the graph type in scope
                // and not a field with the same name on a different graph type
                // it is possible, though unlikely, that a directive execution could swap out the field reference
                if (docPart.Field.Parent != fieldContainer)
                {
                    this.ValidationError(
                        context,
                        $"Invalid field reference. The document field '{docPart.Name.ToString()}' references a field that belongs " +
                        $"to '{docPart.Field.Parent.Name}'. It should reference the field belonging to '{selectionSetGraphType.Name}' ");
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.3.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Field-Selections-on-Objects-Interfaces-and-Unions-Types";
    }
}