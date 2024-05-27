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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Applies a given format pattern to formal graph type names for the target schema.
    /// </summary>
    internal class ApplyGraphTypeNameFormatRule : NameFormatRuleBase, ISchemaItemFormatRule
    {
        private readonly TextFormatOptions _formatOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyGraphTypeNameFormatRule"/> class.
        /// </summary>
        /// <param name="formatOption">The format option.</param>
        public ApplyGraphTypeNameFormatRule(TextFormatOptions formatOption)
        {
            _formatOption = formatOption;
        }

        /// <inheritdoc />
        public TSchemaItemType Execute<TSchemaItemType>(TSchemaItemType schemaItem)
            where TSchemaItemType : ISchemaItem
        {
            // ensure all path segments of the virtual type are
            // named according to the rules of this schema
            switch (schemaItem)
            {
                case VirtualObjectGraphType virtualType:
                    var newName = VirtualObjectGraphType.MakeSafeTypeNameFromItemPath(
                        virtualType.ItemPathTemplate,
                        (segment) => this.FormatText(segment, _formatOption));

                    schemaItem = (TSchemaItemType)virtualType.Clone(newName);
                    break;

                // do nothing with directives
                // when naming graph types
                // they name under the explicit directive rule
                case IDirective directive:
                    break;

                case IGraphType graphType:
                    var formattedName = this.FormatGraphTypeName(graphType.Name, _formatOption);
                    schemaItem = (TSchemaItemType)graphType.Clone(formattedName);
                    break;
            }

            return schemaItem;
        }
    }
}
