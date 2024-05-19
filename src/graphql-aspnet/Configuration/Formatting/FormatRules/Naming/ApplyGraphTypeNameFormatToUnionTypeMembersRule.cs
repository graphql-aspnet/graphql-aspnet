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
    /// Applies the given format pattern to the graph type names contained in a union type.
    /// </summary>
    public class ApplyGraphTypeNameFormatToUnionTypeMembersRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyGraphTypeNameFormatToUnionTypeMembersRule"/> class.
        /// </summary>
        /// <param name="formatOption">The format option.</param>
        public ApplyGraphTypeNameFormatToUnionTypeMembersRule(TextFormatOptions formatOption)
        {
            _formatOption = formatOption;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            if (schemaItem is IUnionGraphType ugt)
            {
                schemaItem = (TSchemaItemType)ugt.Clone(
                    possibleGraphTypeNameFormatter: (typeName)
                            => this.FormatGraphTypeName(typeName, _formatOption));
            }

            return schemaItem;
        }
    }
}
