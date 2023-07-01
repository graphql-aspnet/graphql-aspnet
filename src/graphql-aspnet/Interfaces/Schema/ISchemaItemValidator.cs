// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    /// <summary>
    /// A runtime validator to check <see cref="ISchemaItem"/> instances to ensure they are
    /// usable at runtime.
    /// </summary>
    internal interface ISchemaItemValidator
    {
        /// <summary>
        /// Validates that the given <paramref name="schemaItem"/> is valid and internally consistant
        /// with the provided schema instance.
        /// </summary>
        /// <param name="schemaItem">The schema item to check.</param>
        /// <param name="schema">The schema to check against.</param>
        void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema);
    }
}