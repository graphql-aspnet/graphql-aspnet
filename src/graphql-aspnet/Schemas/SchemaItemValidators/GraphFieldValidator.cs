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

    /// <summary>
    /// A runtime validator that will inspect and ensure the integrity of a <see cref="IGraphField"/>
    /// before a schema is placed online.
    /// </summary>
    internal class GraphFieldValidator : BaseSchemaItemValidator
    {
        /// <summary>
        /// Gets the singular instnace of this validator.
        /// </summary>
        /// <value>The instance.</value>
        public static ISchemaItemValidator Instance { get; } = new GraphFieldValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="GraphFieldValidator"/> class from being created.
        /// </summary>
        private GraphFieldValidator()
        {
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema)
        {
        }
    }
}