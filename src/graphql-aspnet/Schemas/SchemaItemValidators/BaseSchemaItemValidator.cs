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

    internal abstract class BaseSchemaItemValidator : ISchemaItemValidator
    {
        /// <inheritdoc />
        public abstract void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema);
    }
}