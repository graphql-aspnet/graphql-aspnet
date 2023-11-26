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
    /// A runtime validator to check that an <see cref="ISchemaItem"/> instance and ensure that it is
    /// usable at runtime.
    /// </summary>
    internal interface ISchemaItemValidator
    {
        /// <summary>
        /// Validates that the given <paramref name="schemaItem"/> is valid and internally consistant
        /// with the provided schema instance. If the <paramref name="schemaItem"/> is invalid in anyway an
        /// exception must be thrown.
        /// </summary>
        /// <param name="schemaItem">The schema item to validate.</param>
        /// <param name="schema">The schema instance that owns <paramref name="schemaItem"/>.</param>
        void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema);
    }
}