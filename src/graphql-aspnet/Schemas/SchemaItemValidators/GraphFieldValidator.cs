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
    using System;
    using GraphQL.AspNet.Interfaces.Schema;

    internal class GraphFieldValidator : BaseSchemaItemValidator
    {
        public override void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema)
        {
        }
    }
}