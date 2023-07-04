// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.SchemaItemValidators
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A base class with common functionality used by many internal schema item validators.
    /// </summary>
    internal abstract class BaseSchemaItemValidator : ISchemaItemValidator
    {
        /// <inheritdoc />
        public abstract void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema);
    }
}