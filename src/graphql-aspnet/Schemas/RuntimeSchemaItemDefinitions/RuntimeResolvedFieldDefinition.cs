// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Templates
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeResolvedFieldDefinition"/>
    /// used to generate new graphql fields via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{Template}")]
    internal class RuntimeResolvedFieldDefinition : BaseRuntimeControllerActionDefinition, IGraphQLRuntimeResolvedFieldDefinition
    {
        /// <summary>
        /// Converts the unresolved field into a resolved field. The newly generated field
        /// will NOT be attached to any schema and will not have an assigned resolver.
        /// </summary>
        /// <param name="fieldTemplate">The field template.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        public static IGraphQLRuntimeResolvedFieldDefinition FromFieldTemplate(IGraphQLRuntimeFieldDefinition fieldTemplate)
        {
            Validation.ThrowIfNull(fieldTemplate, nameof(fieldTemplate));
            var field = new RuntimeResolvedFieldDefinition(
                fieldTemplate.Options,
                fieldTemplate.Route);

            foreach (var attrib in fieldTemplate.Attributes)
                field.AddAttribute(attrib);

            return field;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeResolvedFieldDefinition" /> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options to which this field is being added.</param>
        /// <param name="route">The full route to use for this item.</param>
        private RuntimeResolvedFieldDefinition(
            SchemaOptions schemaOptions,
            SchemaItemPath route)
            : base(schemaOptions, route)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeResolvedFieldDefinition" /> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options to which this field is being added.</param>
        /// <param name="collection">The schema collection this item will belong to.</param>
        /// <param name="pathTemplate">The path template identifying this item.</param>
        public RuntimeResolvedFieldDefinition(
            SchemaOptions schemaOptions,
            SchemaItemCollections collection,
            string pathTemplate)
            : base(schemaOptions,  collection, pathTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeResolvedFieldDefinition"/> class.
        /// </summary>
        /// <param name="parentFieldBuilder">The parent field builder to which this new, resolved field
        /// will be appended.</param>
        /// <param name="fieldSubTemplate">The template part to append to the parent field's template.</param>
        public RuntimeResolvedFieldDefinition(
            IGraphQLRuntimeFieldDefinition parentFieldBuilder,
            string fieldSubTemplate)
            : base(parentFieldBuilder, fieldSubTemplate)
        {
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            var (collection, path) = this.Route;
            switch (collection)
            {
                case SchemaItemCollections.Query:
                    return new QueryRootAttribute(path, this.ReturnType);

                case SchemaItemCollections.Mutation:
                    return new MutationRootAttribute(path, this.ReturnType);
            }

            return null;
        }

        /// <inheritdoc />
        public Delegate Resolver { get; set; }

        /// <inheritdoc />
        public Type ReturnType { get; set; }
    }
}