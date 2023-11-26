// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.SchemaItemValidators
{
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A runtime schema item validator that performs no validation.
    /// </summary>
    internal class NoValidationSchemaItemValidator : BaseSchemaItemValidator
    {
        /// <summary>
        /// Gets the singular instnace of this validator.
        /// </summary>
        /// <value>The instance.</value>
        public static ISchemaItemValidator Instance { get; } = new NoValidationSchemaItemValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="NoValidationSchemaItemValidator"/> class from being created.
        /// </summary>
        private NoValidationSchemaItemValidator()
        {
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema)
        {
            // do nothing
        }
    }
}