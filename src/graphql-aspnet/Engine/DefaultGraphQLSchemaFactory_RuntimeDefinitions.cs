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
    using System;
    using System.Reflection.Metadata;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// The default schema factory, capable of creating instances of
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
            switch (itemDefinition)
            {
                case IGraphQLRuntimeResolvedFieldDefinition fieldDef:
                    this.AddRuntimeFieldDefinition(fieldDef);
                    break;

                case IGraphQLRuntimeDirectiveDefinition directiveDef:
                    this.AddRuntimeDirectiveDefinition(directiveDef);
                    break;

                    // TODO: Add support for directives
                    // TODO: Add warning log entries for unsupported item defs.
            }
        }

        /// <summary>
        /// Adds a new directive to the schema based on the runtime definition created during program startup.
        /// </summary>
        /// <param name="directiveDefinition">The directive definition.</param>
        protected virtual void AddRuntimeDirectiveDefinition(IGraphQLRuntimeDirectiveDefinition directiveDefinition)
        {
            Validation.ThrowIfNull(directiveDefinition, nameof(directiveDefinition));
            var template = new RuntimeGraphDirectiveTemplate(directiveDefinition);

            template.Parse();
            template.ValidateOrThrow();

            var maker = this.MakerFactory.CreateTypeMaker(kind: TypeKind.DIRECTIVE);
            var result = maker.CreateGraphType(template);
            this.AddMakerResult(result);
        }

        /// <summary>
        /// Adds a new field to the schema based on the runtime definition created during program startup.
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