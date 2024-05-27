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
    /// Applies a name given format pattern to each field in the targetschema.
    /// </summary>
    internal class ApplyFieldNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyFieldNameFormatRule"/> class.
        /// </summary>
        /// <param name="formatOption">The format option.</param>
        public ApplyFieldNameFormatRule(TextFormatOptions formatOption)
        {
            _formatOption = formatOption;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IGraphField field)
            {
                var formattedName = this.FormatText(field.Name, _formatOption);
                schemaItem = (TSchemaItemType)field.Clone(fieldName: formattedName);
            }

            return schemaItem;
        }
    }
}
