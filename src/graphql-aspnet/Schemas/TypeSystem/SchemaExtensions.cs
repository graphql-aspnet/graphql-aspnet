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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// Extensions for working <see cref="ISchema"/> objects.
    /// </summary>
    public static class SchemaExtensions
    {
        /// <summary>
        /// Generates an enumerable that containing all <see cref="ISchemaItem" /> instances known to this schema.
        /// </summary>
        /// <param name="schema">The schema to inspecet.</param>
        /// <param name="includeDirectives">if set to <c>true</c> <see cref="IDirective"/> schema items
        /// will be included in the enumeration.</param>
        /// <returns>IEnumerable&lt;ISchemaItem&gt;.</returns>
        public static IEnumerable<ISchemaItem> AllSchemaItems(this ISchema schema, bool includeDirectives = false)
        {
            Validation.ThrowIfNull(schema, nameof(schema));

            // schema first
            yield return schema;

            // process each graph item except directives unless allowed
            var graphTypesToProcess = schema.KnownTypes.Where(x =>
                (includeDirectives || x.Kind != TypeKind.DIRECTIVE));

            foreach (var graphType in graphTypesToProcess)
            {
                yield return graphType;

                switch (graphType)
                {
                    case IEnumGraphType enumType:
                        // each option on each enum
                        foreach (var option in enumType.Values)
                            yield return option.Value;
                        break;

                    case IInputObjectGraphType inputObject:
                        foreach (var inputField in inputObject.Fields)
                            yield return inputField;
                        break;

                    // object graph types and interface graph types
                    case IGraphFieldContainer fieldContainer:
                        foreach (var field in fieldContainer.Fields)
                        {
                            yield return field;

                            // each argument on each field
                            foreach (var argument in field.Arguments)
                                yield return argument;
                        }

                        break;

                    case IDirective directive:
                        foreach (var argument in directive.Arguments)
                            yield return argument;
                        break;
                }
            }
        }
    }
}