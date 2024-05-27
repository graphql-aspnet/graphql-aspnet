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
    /// Applies a given name format pattern to enum values within the target schema.
    /// </summary>
    internal class ApplyEnumNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyEnumNameFormatRule"/> class.
        /// </summary>
        /// <param name="formatOption">The format option.</param>
        public ApplyEnumNameFormatRule(TextFormatOptions formatOption)
        {
            _formatOption = formatOption;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IEnumValue enumValue)
            {
                var formattedName = this.FormatText(enumValue.Name, _formatOption);
                schemaItem = (TSchemaItemType)enumValue.Clone(valueName: formattedName);
            }

            return schemaItem;
        }
    }
}
