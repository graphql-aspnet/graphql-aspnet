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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.TypeMakers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeMakers;

    /// <summary>
    /// A schema creation factory that understands and can generate subscription types.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema being generated.</typeparam>
    public class SubscriptionEnabledGraphQLSchemaFactory<TSchema> : DefaultGraphQLSchemaFactory<TSchema>
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        protected override GraphTypeMakerFactory CreateMakerFactory()
        {
            return new SubscriptionEnabledGraphTypeMakerFactory(this.Schema);
        }

        /// <inheritdoc />
        protected override void AddRuntimeFieldDefinition(IGraphQLRuntimeResolvedFieldDefinition fieldDefinition)
        {
            Validation.ThrowIfNull(fieldDefinition, nameof(fieldDefinition));

            if (fieldDefinition is IGraphQLSubscriptionEnabledRuntimeResolvedFieldDefinition subField)
            {
                var template = new RuntimeSubscriptionGraphControllerTemplate(subField);
                template.Parse();
                template.ValidateOrThrow();

                this.AddController(template);
                return;
            }

            base.AddRuntimeFieldDefinition(fieldDefinition);
        }
    }
}