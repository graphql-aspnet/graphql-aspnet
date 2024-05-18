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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A rule that defines a specific set of business logic that can be executed against
    /// a schema item just before its inserted intoa schema.
    /// </summary>
    public interface ISchemaItemFormatRule
    {
        /// <summary>
        /// Executes this rule against the target schema item
        /// </summary>
        /// <typeparam name="T">The actual type of the schema item being processed.</typeparam>
        /// <param name="schemaItem">The schema item.</param>
        /// <returns>The original schema item or a modified version of the schema item.</returns>
        public T Execute<T>(T schemaItem)
            where T : ISchemaItem;
    }
}
