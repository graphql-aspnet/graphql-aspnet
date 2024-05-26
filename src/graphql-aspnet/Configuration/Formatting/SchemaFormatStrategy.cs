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