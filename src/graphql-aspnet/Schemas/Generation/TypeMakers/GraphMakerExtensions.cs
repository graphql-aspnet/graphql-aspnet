// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Extension methods used during graph type creation.
    /// </summary>
    public static class GraphMakerExtensions
    {
        /// <summary>
        /// Creates a set of applied directive from the directive templates
        /// on the given item template.
        /// </summary>
        /// <param name="template">The template to extract directives from.</param>
        /// <returns>IAppliedDirectiveCollection.</returns>
        public static IAppliedDirectiveCollection CreateAppliedDirectives(this ISchemaItemTemplate template)
        {
            Validation.ThrowIfNull(template, nameof(template));
            return template.AppliedDirectives.CreateAppliedDirectives();
        }

        /// <summary>
        /// Creates a set of applied directive from the provided templates.
        /// </summary>
        /// <param name="directiveTemplates">The directive templates to convert.</param>
        /// <returns>IAppliedDirectiveCollection.</returns>
        public static IAppliedDirectiveCollection CreateAppliedDirectives(this IEnumerable<IAppliedDirectiveTemplate> directiveTemplates)
        {
            Validation.ThrowIfNull(directiveTemplates, nameof(directiveTemplates));

            var directives = new AppliedDirectiveCollection();
            foreach (var directiveTemplate in directiveTemplates)
                directives.Add(directiveTemplate.CreateAppliedDirective());

            return directives;
        }
    }
}