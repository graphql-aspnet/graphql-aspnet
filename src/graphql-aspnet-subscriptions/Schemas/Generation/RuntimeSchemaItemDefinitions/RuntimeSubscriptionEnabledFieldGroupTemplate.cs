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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;

    /// <summary>
    /// A <see cref="RuntimeFieldGroupTemplate"/> with the ability to create subscription
    /// enabled minimal api fields.
    /// </summary>
    public sealed class RuntimeSubscriptionEnabledFieldGroupTemplate : RuntimeFieldGroupTemplateBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSubscriptionEnabledFieldGroupTemplate"/> class.
        /// </summary>
        /// <param name="options">The primary schema options where this field (and its children) will be
        /// instantiated.</param>
        /// <param name="pathTemplate">The path template describing this field.</param>
        public RuntimeSubscriptionEnabledFieldGroupTemplate(
            SchemaOptions options, string pathTemplate)
            : base(
                  options,
                  ItemPathRoots.Subscription,
                  pathTemplate)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSubscriptionEnabledFieldGroupTemplate" /> class.
        /// </summary>
        /// <param name="parentField">The field from which this entity is being added.</param>
        /// <param name="fieldSubTemplate">The partial path template to be appended to
        /// the parent's already defined template.</param>
        public RuntimeSubscriptionEnabledFieldGroupTemplate(
            IGraphQLRuntimeFieldGroupDefinition parentField, string fieldSubTemplate)
            : base(parentField, fieldSubTemplate)
        {
        }

        /// <inheritdoc />
        public override IGraphQLRuntimeFieldGroupDefinition MapChildGroup(string pathTemplate)
        {
            return new RuntimeSubscriptionEnabledFieldGroupTemplate(this, pathTemplate);
        }

        /// <inheritdoc />
        public override IGraphQLRuntimeResolvedFieldDefinition MapField(string pathTemplate)
        {
            var field = new RuntimeSubscriptionEnabledResolvedFieldDefinition(this, pathTemplate, null);
            this.Options.AddRuntimeSchemaItem(field);
            return field;
        }
    }
}