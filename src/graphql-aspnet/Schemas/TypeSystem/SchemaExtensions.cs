// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Extensions for working <see cref="ISchema"/> objects.
    /// </summary>
    public static class SchemaExtensions
    {
        /// <summary>
        /// An enumeration containing all <see cref="ISchemaItem"/> instances known to this schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>IEnumerable&lt;ISchemaItem&gt;.</returns>
        public static IEnumerable<ISchemaItem> AllSchemaItems(this ISchema schema)
        {
            // schema first
            yield return schema;

            // all declared operations
            foreach (var operationEntry in schema.OperationTypes)
                yield return operationEntry.Value;

            // process each graph type except directives
            var graphTypesToProcess = schema.KnownTypes.Where(x =>
                x.Kind != TypeKind.DIRECTIVE
                && !(x is IGraphOperation));

            foreach (var graphType in graphTypesToProcess)
            {
                yield return graphType;

                if (graphType is IEnumGraphType enumType)
                {
                    // each option on each enum
                    foreach (var option in enumType.Values)
                        yield return option.Value;
                }
                else if (graphType is IGraphFieldContainer fieldContainer)
                {
                    // each field in each graph type
                    foreach (var field in fieldContainer.Fields)
                    {
                        yield return field;

                        // each argument on each field
                        foreach (var argument in field.Arguments)
                            yield return argument;
                    }
                }
            }
        }
    }
}