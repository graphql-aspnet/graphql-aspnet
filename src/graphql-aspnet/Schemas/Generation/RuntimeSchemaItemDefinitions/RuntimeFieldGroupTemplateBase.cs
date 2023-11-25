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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeFieldGroupDefinition"/>
    /// used to generate new graphql fields via a minimal api style of coding.
    /// </summary>
    public abstract class RuntimeFieldGroupTemplateBase : RuntimeControllerActionDefinitionBase, IGraphQLRuntimeFieldGroupDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFieldGroupTemplateBase" /> class.
        /// </summary>
        /// <param name="options">The schema options that will own the fields created from
        /// this builder.</param>
        /// <param name="collection">The schema collection this item will belong to.</param>
        /// <param name="pathTemplate">The path template identifying this item.</param>
        protected RuntimeFieldGroupTemplateBase(
            SchemaOptions options,
            ItemPathRoots collection,
            string pathTemplate)
            : base(options, collection, pathTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFieldGroupTemplateBase" /> class.
        /// </summary>
        /// <param name="parentField">The field from which this entity is being added.</param>
        /// <param name="partialPathTemplate">The partial path template defined for this
        /// individual entity.</param>
        protected RuntimeFieldGroupTemplateBase(
            IGraphQLRuntimeFieldGroupDefinition parentField,
            string partialPathTemplate)
            : base(parentField, partialPathTemplate)
        {
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            return null;
        }

        /// <inheritdoc />
        public abstract IGraphQLRuntimeResolvedFieldDefinition MapField(string pathTemplate);

        /// <inheritdoc />
        public abstract IGraphQLRuntimeFieldGroupDefinition MapChildGroup(string pathTemplate);
    }
}