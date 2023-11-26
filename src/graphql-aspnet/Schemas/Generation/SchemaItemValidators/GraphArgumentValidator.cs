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
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A validator of a completed and schema-attached graph argument that ensures it can function as
    /// expected in the target schema.
    /// </summary>
    internal class GraphArgumentValidator : BaseSchemaItemValidator
    {
        /// <summary>
        /// Gets the singular instnace of this validator.
        /// </summary>
        /// <value>The instance.</value>
        public static ISchemaItemValidator Instance { get; } = new GraphArgumentValidator();

        /// <summary>
        /// Prevents a default instance of the <see cref="GraphArgumentValidator"/> class from being created.
        /// </summary>
        private GraphArgumentValidator()
        {
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(ISchemaItem schemaItem, ISchema schema)
        {
            Validation.ThrowIfNull(schemaItem, nameof(schemaItem));
            Validation.ThrowIfNull(schema, nameof(schema));

            var argument = schemaItem as IGraphArgument;
            if (argument == null)
            {
                throw new InvalidCastException(
                    $"Unable to validate argument. Expected type " +
                    $"'{typeof(IGraphArgument).FriendlyName()}' but got '{schema?.GetType().FriendlyName() ?? "-none-"}'");
            }

            if (argument.ObjectType == null || argument.ObjectType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The argument '{argument.Name}' on  '{argument.Parent?.Route.Path ?? "~unknown~"}' is of type  '{argument.ObjectType?.FriendlyName() ?? "~null~"}', " +
                    $"which is an interface. Interfaces cannot be used as input arguments to any graph type.");
            }

            // the type MUST be in the schema
            var foundItem = schema.KnownTypes.FindGraphType(argument.ObjectType, TypeSystem.TypeKind.INPUT_OBJECT);
            if (foundItem == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The argument '{argument.Name}' on  '{argument.Parent?.Route.Path ?? "~unknown~"}' is declared as a {argument.ObjectType.FriendlyName()}, " +
                    $"which is not included in the schema as an acceptable input type (e.g. Scalar, Enum or Input Object). Ensure your type is included in the schema.");
            }
        }
    }
}