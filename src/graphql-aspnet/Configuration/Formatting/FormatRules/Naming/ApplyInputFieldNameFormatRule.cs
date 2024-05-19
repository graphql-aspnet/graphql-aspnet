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
    /// Applies the given format option to fields on INPUT_OBJECTs for the given schema.
    /// </summary>
    internal class ApplyInputFieldNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyInputFieldNameFormatRule"/> class.
        /// </summary>
        /// <param name="formatOption">The format option.</param>
        public ApplyInputFieldNameFormatRule(TextFormatOptions formatOption)
        {
            _formatOption = formatOption;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IInputGraphField field)
            {
                var formattedName = this.FormatText(field.Name, _formatOption);
                schemaItem = (TSchemaItemType)field.Clone(fieldName: formattedName);
            }

            return schemaItem;
        }
    }
}
