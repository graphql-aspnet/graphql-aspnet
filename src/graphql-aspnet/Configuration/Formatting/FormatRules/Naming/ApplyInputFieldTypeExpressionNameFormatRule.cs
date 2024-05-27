// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting.FormatRules
{
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Applies a graph type name format pattern to the type expression on each field on the INPUT_OBJECTs in
    /// the target schema.
    /// </summary>
    internal class ApplyInputFieldTypeExpressionNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyInputFieldTypeExpressionNameFormatRule" /> class.
        /// </summary>
        /// <param name="typeExpressionGraphTypeFormat">The format to use for the type name
        /// referenced in the fields type expression.</param>
        public ApplyInputFieldTypeExpressionNameFormatRule(TextFormatOptions typeExpressionGraphTypeFormat)
        {
            _formatOption = typeExpressionGraphTypeFormat;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IInputGraphField field)
            {
                var typeExpression = field.TypeExpression;
                var formattedName = this.FormatGraphTypeName(typeExpression.TypeName, _formatOption);
                typeExpression = typeExpression.Clone(formattedName);

                schemaItem = (TSchemaItemType)field.Clone(typeExpression: typeExpression);
            }

            return schemaItem;
        }
    }
}
