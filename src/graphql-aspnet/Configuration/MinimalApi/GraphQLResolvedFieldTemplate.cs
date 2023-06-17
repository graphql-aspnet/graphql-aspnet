// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.MinimalApi
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLResolvedFieldTemplate"/>
    /// used to generate new graphql fields via a minimal api style of coding.
    /// </summary>
    internal class GraphQLResolvedFieldTemplate : BaseGraphQLFieldTemplate, IGraphQLResolvedFieldTemplate
    {
        /// <summary>
        /// Converts the unresolved field into a resolved field. The newly generated field
        /// will NOT be attached to any schema and will not have an assigned resolver.
        /// </summary>
        /// <param name="fieldTemplate">The field template.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLResolvedFieldTemplate FromFieldTemplate(IGraphQLFieldTemplate fieldTemplate)
        {
            Validation.ThrowIfNull(fieldTemplate, nameof(fieldTemplate));
            var field = new GraphQLResolvedFieldTemplate(fieldTemplate.Options, fieldTemplate.Template);

            foreach (var attrib in fieldTemplate.Attributes)
                field.Attributes.Add(attrib);

            foreach (var kvp in fieldTemplate)
                field.Add(kvp.Key, kvp.Value);

            return field;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLResolvedFieldTemplate"/> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options to which this field is being added.</param>
        /// <param name="fullPathTemplate">The full path template describing where the field will live.</param>
        public GraphQLResolvedFieldTemplate(
            SchemaOptions schemaOptions,
            string fullPathTemplate)
            : base(schemaOptions, fullPathTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphQLResolvedFieldTemplate"/> class.
        /// </summary>
        /// <param name="parentFieldBuilder">The parent field builder to which this new, resolved field
        /// will be appended.</param>
        /// <param name="fieldSubTemplate">The template part to append to the parent field's template.</param>
        public GraphQLResolvedFieldTemplate(
            IGraphQLFieldTemplate parentFieldBuilder,
            string fieldSubTemplate)
            : base(parentFieldBuilder, fieldSubTemplate)
        {
        }

        /// <inheritdoc />
        public Delegate Resolver { get; set; }

        /// <inheritdoc />
        public Type ReturnType { get; set; }
    }
}