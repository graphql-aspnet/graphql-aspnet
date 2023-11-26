// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeResolvedFieldDefinition"/>
    /// used to generate new graphql fields via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{ItemPath.Path}")]
    public class RuntimeResolvedFieldDefinition : RuntimeControllerActionDefinitionBase, IGraphQLRuntimeResolvedFieldDefinition
    {
        /// <summary>
        /// Converts the unresolved field into a resolved field. The newly generated field
        /// will NOT be attached to any schema and will not have an assigned resolver.
        /// </summary>
        /// <param name="fieldTemplate">The field template.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        internal static IGraphQLRuntimeResolvedFieldDefinition FromFieldTemplate(IGraphQLRuntimeFieldGroupDefinition fieldTemplate)
        {
            Validation.ThrowIfNull(fieldTemplate, nameof(fieldTemplate));
            var field = new RuntimeResolvedFieldDefinition(
                fieldTemplate.Options,
                fieldTemplate.ItemPath);

            foreach (var attrib in fieldTemplate.Attributes)
                field.AddAttribute(attrib);

            return field;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeResolvedFieldDefinition" /> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options to which this field is being added.</param>
        /// <param name="itemPath">The full path to use for this item.</param>
        protected RuntimeResolvedFieldDefinition(
            SchemaOptions schemaOptions,
            ItemPath itemPath)
            : base(schemaOptions, itemPath)
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
            ItemPathRoots collection,
            string pathTemplate)
            : base(schemaOptions, collection, pathTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeResolvedFieldDefinition"/> class.
        /// </summary>
        /// <param name="parentFieldBuilder">The parent field builder to which this new, resolved field
        /// will be appended.</param>
        /// <param name="fieldSubTemplate">The template part to append to the parent field's template.</param>
        public RuntimeResolvedFieldDefinition(
            IGraphQLRuntimeFieldGroupDefinition parentFieldBuilder,
            string fieldSubTemplate)
            : base(parentFieldBuilder, fieldSubTemplate)
        {
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            var (collection, path) = this.ItemPath;
            switch (collection)
            {
                case ItemPathRoots.Query:
                    return new QueryRootAttribute(path, this.ReturnType)
                    {
                        InternalName = this.InternalName,
                    };

                case ItemPathRoots.Mutation:
                    return new MutationRootAttribute(path, this.ReturnType)
                    {
                        InternalName = this.InternalName,
                    };
            }

            return null;
        }

        /// <inheritdoc />
        public Delegate Resolver { get; set; }

        /// <inheritdoc />
        public Type ReturnType { get; set; }

        /// <inheritdoc />
        public string InternalName { get; set; }
    }
}