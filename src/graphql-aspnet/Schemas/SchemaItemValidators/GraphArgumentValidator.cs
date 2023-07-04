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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

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
            var argument = schemaItem as IGraphArgument;
            if (argument == null)
            {
                throw new InvalidCastException(
                    $"Unable to validate argument. Expected type " +
                    $"'{typeof(IGraphArgument).FriendlyName()}' but got '{schema?.GetType().FriendlyName() ?? "-none-"}'");
            }

            if (argument.ObjectType.IsInterface)
            {
                throw new GraphTypeDeclarationException(
                    $"The argument '{argument.Name}' on  '{argument.Parent.Route.Path}' is of type  '{argument.ObjectType.FriendlyName()}', " +
                    $"which is an interface. Interfaces cannot be used as input arguments to any graph type.");
            }
        }
    }
}