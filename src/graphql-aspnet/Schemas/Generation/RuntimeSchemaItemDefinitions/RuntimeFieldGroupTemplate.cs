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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;

    /// <summary>
    /// An internal implementation of the <see cref="IGraphQLRuntimeFieldGroupDefinition"/>
    /// used to generate new graphql fields via a minimal api style of coding.
    /// </summary>
    [DebuggerDisplay("{ItemPath.Path}")]
    public sealed class RuntimeFieldGroupTemplate : RuntimeFieldGroupTemplateBase, IGraphQLRuntimeFieldGroupDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFieldGroupTemplate" /> class.
        /// </summary>
        /// <param name="options">The schema options that will own the fields created from
        /// this builder.</param>
        /// <param name="collection">The schema collection this item will belong to.</param>
        /// <param name="pathTemplate">The path template identifying this item.</param>
        public RuntimeFieldGroupTemplate(
            SchemaOptions options,
            ItemPathRoots collection,
            string pathTemplate)
            : base(options, collection, pathTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeFieldGroupTemplate" /> class.
        /// </summary>
        /// <param name="parentField">The field from which this entity is being added.</param>
        /// <param name="fieldSubTemplate">The partial path template to be appended to
        /// the parent's already defined template.</param>
        public RuntimeFieldGroupTemplate(
            IGraphQLRuntimeFieldGroupDefinition parentField, string fieldSubTemplate)
            : base(parentField, fieldSubTemplate)
        {
        }

        /// <inheritdoc />
        public override IGraphQLRuntimeResolvedFieldDefinition MapField(string pathTemplate)
        {
            var field = new RuntimeResolvedFieldDefinition(this, pathTemplate);
            this.Options.AddRuntimeSchemaItem(field);
            return field;
        }

        /// <inheritdoc />
        public override IGraphQLRuntimeFieldGroupDefinition MapChildGroup(string pathTemplate)
        {
            return new RuntimeFieldGroupTemplate(this, pathTemplate);
        }
    }
}