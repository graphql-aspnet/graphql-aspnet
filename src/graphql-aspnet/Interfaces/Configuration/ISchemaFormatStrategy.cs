// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A strategy that defines specific rules and overrides to apply to schema items as they
    /// are generated and added to a schema being built.
    /// </summary>
    public interface ISchemaFormatStrategy
    {
        /// <summary>
        /// Applies custom, programatic level formatting rules (name formatting, nullability standards etc.) to the
        /// target schema item before it is added to the schema.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="schemaItem"/> is not yet added to the schema
        /// and may not be be fully populated.
        /// For example, a field will not yet have a parent assignment and graph types
        /// will not have any rendered fields when this method is processed.
        /// </para>
        /// <para>
        /// Schema items are immutable for any essential data values. Use the various
        /// implementations of <c>.Clone()</c> to create copies of a schema item with
        /// updated values.
        /// </para>
        /// <para>
        /// Applied changes are accepted as is with no further validation, ensure your
        /// formats are consistant with the target schema's expectations.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The type of schema item being formatted.</typeparam>
        /// <param name="configuration">The complete configuration settings setup
        /// at application start for the target schema.</param>
        /// <param name="schemaItem">The schema item to apply formatting rules to.</param>
        /// <returns>The updated or altered schema item instance that was formatted.</returns>
        T ApplySchemaItemRules<T>(ISchemaConfiguration configuration, T schemaItem)
            where T : ISchemaItem;

        /// <summary>
        /// Formats the provided string as if it were the name of an entity
        /// according to the rules of the schema.
        /// </summary>
        /// <param name="name">The name to format.</param>
        /// <param name="target">The target schema item entity type.</param>
        /// <returns>A formatted string.</returns>
        string FormatSchemaItemName(string name, NameFormatCategory target);
    }
}
