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
    /// Applies a graph type name format pattern to the type expression on each argument encounterd
    /// on fields in OBJECT and INTERFACE types as well as those defined on DIRECTIVE types for the target schema.
    /// </summary>
    internal class ApplyArgumentTypeExpressionNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyArgumentTypeExpressionNameFormatRule" /> class.
        /// </summary>
        /// <param name="typeExpressionGraphTypeFormat">The format to use for the type name
        /// referneced in the argument's type expression.</param>
        public ApplyArgumentTypeExpressionNameFormatRule(TextFormatOptions typeExpressionGraphTypeFormat)
        {
            _formatOption = typeExpressionGraphTypeFormat;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IGraphArgument argument)
            {
                var typeExpression = argument.TypeExpression;
                var formattedName = this.FormatGraphTypeName(typeExpression.TypeName, _formatOption);
                typeExpression = typeExpression.Clone(formattedName);

                schemaItem = (TSchemaItemType)argument.Clone(typeExpression: typeExpression);
            }

            return schemaItem;
        }
    }
}
