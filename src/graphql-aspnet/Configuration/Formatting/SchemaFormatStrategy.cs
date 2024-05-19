// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Formatting
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A strategy, employed by a schema, to make programatic alterations
    /// to schema items as they are being constructed.
    /// </summary>
    public class SchemaFormatStrategy : ISchemaFormatStrategy
    {
        /// <summary>
        /// Creates a format strategy with all internal default rules. All name format rules
        /// will be overriden with the given format option.
        /// </summary>
        /// <param name="formatOption">The single format option to use for all generated names.</param>
        /// <returns>SchemaFormatStrategy.</returns>
        public static ISchemaFormatStrategy CreateEmpty(TextFormatOptions formatOption)
        {
            return CreateEmpty(formatOption, formatOption, formatOption, formatOption, formatOption);
        }

        /// <summary>
        /// Creates a format strategy with all internal default rules. Name format overrides
        /// can be applied if needed.
        /// </summary>
        /// <param name="typeNameFormat">The name format to use for graph type names.</param>
        /// <param name="fieldNameFormat">The name format to use for fields.</param>
        /// <param name="enumValueNameFormat">The name format to use for enum values.</param>
        /// <param name="argumentNameFormat">The name format to use for arguments on fields and directives.</param>
        /// <param name="directiveNameFormat">The name format to use for directive type names.</param>
        /// <returns>SchemaFormatStrategy.</returns>
        public static ISchemaFormatStrategy CreateEmpty(
            TextFormatOptions? typeNameFormat = TextFormatOptions.ProperCase,
            TextFormatOptions? fieldNameFormat = TextFormatOptions.CamelCase,
            TextFormatOptions? enumValueNameFormat = TextFormatOptions.UpperCase,
            TextFormatOptions? argumentNameFormat = TextFormatOptions.CamelCase,
            TextFormatOptions? directiveNameFormat = TextFormatOptions.CamelCase)
        {
            var builder = new SchemaFormatStrategyBuilder()
                .Clear();

            if (typeNameFormat.HasValue)
                builder = builder.WithGraphTypeNameFormat(typeNameFormat.Value);
            if (fieldNameFormat.HasValue)
                builder = builder.WithFieldNameFormat(fieldNameFormat.Value);
            if (enumValueNameFormat.HasValue)
                builder = builder.WithEnumValueFormat(enumValueNameFormat.Value);
            if (directiveNameFormat.HasValue)
                builder = builder.WithDirectiveNameFormat(directiveNameFormat.Value);
            if (argumentNameFormat.HasValue)
                builder = builder.WithFieldArgumentNameFormat(argumentNameFormat.Value);

            return builder.Build();
        }

        private List<ISchemaItemFormatRule> _formatRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFormatStrategy" /> class.
        /// </summary>
        /// <param name="formatRules">The format rules to apply to schema items passing through
        /// this instance.</param>
        public SchemaFormatStrategy(params ISchemaItemFormatRule[] formatRules)
        {
            _formatRules = formatRules.ToList() ?? [];
        }

        /// <inheritdoc />
        public virtual T ApplySchemaItemRules<T>(ISchemaConfiguration configuration, T schemaItem)
            where T : ISchemaItem
        {
            foreach (var rule in _formatRules)
                schemaItem = rule.Execute(schemaItem);

            return schemaItem;
        }
    }
}