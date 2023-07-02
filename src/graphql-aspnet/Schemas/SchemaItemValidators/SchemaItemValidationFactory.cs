// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.SchemaItemValidators;

using GraphQL.AspNet.Interfaces.Schema;

/// <summary>
/// A factory for producing validator instances that can validate a given <see cref="ISchemaItem"/>
/// consistancy against a target schema.
/// </summary>
internal static class SchemaItemValidationFactory
{
    /// <summary>
    /// Creates a validator instance for the given schema item.
    /// </summary>
    /// <param name="schemaItem">The schema item.</param>
    /// <returns>ISchemaItemValidator.</returns>
    public static ISchemaItemValidator CreateValidator(ISchemaItem schemaItem)
    {
        switch (schemaItem)
        {
            case IGraphArgument _:
                return GraphArgumentValidator.Instance;

            default:
                return NoValidationSchemaItemValidator.Instance;
        }
    }
}