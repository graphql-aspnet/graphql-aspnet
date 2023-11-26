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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A runtime field definition that supports additional required data items to properly setup a subscription via
    /// a "minimal api" call.
    /// </summary>
    public class RuntimeSubscriptionEnabledResolvedFieldDefinition : RuntimeResolvedFieldDefinition, IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition
    {
        private string _customEventName;

        /// <summary>
        /// Converts the unresolved field into a resolved field. The newly generated field
        /// will NOT be attached to any schema and will not have an assigned resolver.
        /// </summary>
        /// <param name="fieldTemplate">The field template.</param>
        /// <returns>IGraphQLResolvedFieldTemplate.</returns>
        internal static IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition FromFieldTemplate(IGraphQLRuntimeFieldGroupDefinition fieldTemplate)
        {
            Validation.ThrowIfNull(fieldTemplate, nameof(fieldTemplate));
            var field = new RuntimeSubscriptionEnabledResolvedFieldDefinition(
                fieldTemplate.Options,
                fieldTemplate.ItemPath,
                null);

            foreach (var attrib in fieldTemplate.Attributes)
                field.AddAttribute(attrib);

            return field;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSubscriptionEnabledResolvedFieldDefinition"/> class.
        /// </summary>
        /// <param name="parentFieldBuilder">The parent field builder to which this new, resolved field
        /// will be appended.</param>
        /// <param name="fieldSubTemplate">The template part to append to the parent field's template.</param>
        /// <param name="eventName">Name of the event that must be published to invoke this subscription
        /// field.</param>
        public RuntimeSubscriptionEnabledResolvedFieldDefinition(
            IGraphQLRuntimeFieldGroupDefinition parentFieldBuilder,
            string fieldSubTemplate,
            string eventName)
            : base(parentFieldBuilder, fieldSubTemplate)
        {
            this.EventName = eventName?.Trim();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSubscriptionEnabledResolvedFieldDefinition" /> class.
        /// </summary>
        /// <param name="schemaOptions">The schema options to which this field is being added.</param>
        /// <param name="route">The full route to use for this item.</param>
        /// <param name="eventName">Name of the event that must be published to invoke this subscription
        /// field.</param>
        private RuntimeSubscriptionEnabledResolvedFieldDefinition(
            SchemaOptions schemaOptions,
            ItemPath route,
            string eventName)
            : base(schemaOptions, route)
        {
            this.EventName = eventName?.Trim();
        }

        /// <inheritdoc />
        protected override Attribute CreatePrimaryAttribute()
        {
            var (collection, path) = this.ItemPath;
            if (collection == ItemPathRoots.Subscription)
            {
                return new SubscriptionRootAttribute(path, this.ReturnType)
                {
                    InternalName = this.InternalName,
                    EventName = this.EventName,
                };
            }

            return base.CreatePrimaryAttribute();
        }

        /// <inheritdoc />
        public string EventName
        {
            get
            {
                return _customEventName;
            }

            set
            {
                _customEventName = value?.Trim();
                if (!string.IsNullOrWhiteSpace(_customEventName))
                    return;

                var (_, path) = this.ItemPath;

                // turns path1/path2/path3 =>  path1_path2_path3
                _customEventName = string.Join(
                    "_",
                    path.Split(
                        Constants.Routing.PATH_SEPERATOR,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }
        }
    }
}