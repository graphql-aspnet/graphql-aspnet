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
    /// Applies a given format pattern to formal name for directives on the target schema.
    /// </summary>
    internal class ApplyDirectiveNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyDirectiveNameFormatRule"/> class.
        /// </summary>
        /// <param name="formatOption">The format option.</param>
        public ApplyDirectiveNameFormatRule(TextFormatOptions formatOption)
        {
            _formatOption = formatOption;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            // ensure all path segments of the virtual type are
            // named according to the rules of this schema
            if (schemaItem is IDirective directive)
            {
                var formattedName = this.FormatText(directive.Name, _formatOption);
                schemaItem = (TSchemaItemType)directive.Clone(formattedName);
            }

            return schemaItem;
        }
    }
}
