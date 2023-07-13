// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    public partial class DefaultGraphQLSchemaFactory<TSchema>
    {
        /// <summary>
        /// Adds a runtime declared field (with its assigned resolver) as a field in the schema.
        /// </summary>
        /// <param name="itemDefinition">The runtime defined item to add to the schema.</param>
        protected virtual void AddRuntimeSchemaItemDefinition(IGraphQLRuntimeSchemaItemDefinition itemDefinition)
        {
            if (itemDefinition is IGraphQLRuntimeResolvedFieldDefinition fieldDef)
                this.AddRuntimeFieldDefinition(fieldDef);
        }

        /// <summary>
        /// Adds the runtime defined field into the schema.
        /// </summary>
        /// <param name="fieldDefinition">The field definition to add.</param>
        protected virtual void AddRuntimeFieldDefinition(IGraphQLRuntimeResolvedFieldDefinition fieldDefinition)
        {
            Validation.ThrowIfNull(fieldDefinition, nameof(fieldDefinition));
            var template = new RuntimeGraphControllerTemplate(fieldDefinition);

            template.Parse();
            template.ValidateOrThrow();

            this.AddController(template);
        }
    }
}