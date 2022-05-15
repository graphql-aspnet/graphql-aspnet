// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// Extension methods for working with <see cref="ISchema"/> objects.
    /// </summary>
    public static class GraphSchemaExtensions
    {
        /// <summary>
        /// Ensures that any directives applied to a custom <see cref="ISchema"/>
        /// via the attribute <see cref="ApplyDirectiveAttribute"/> are included
        /// as part of the schema's actual applied directives collection.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public static void EnsureAppliedDirectives(this ISchema schema)
        {
            if (schema == null)
                return;

            foreach (var directive in schema.GetType().ExtractAppliedDirectives())
            {
                schema.AppliedDirectives.Add(directive);
            }
        }
    }
}