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
    using System.Collections.Generic;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas.SchemaItemValidators;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// The default schema factory, capable of creating singleton instances of
    /// schemas, fully populated and ready to serve requests.
    /// </summary>
    public partial class DefaultGraphQLSchemaFactory<TSchema>
    {
        /// <summary>
        /// Validates each registered type, field, argument and directive to ensure that its
        /// internally consistance with itself and that the schema is in a usable state.
        /// </summary>
        protected virtual void ValidateSchemaIntegrity()
        {
            var allItems = this.Schema.AllSchemaItems(includeDirectives: true);

            foreach (var item in allItems)
            {
                var validator = SchemaItemValidationFactory.CreateValidator(item);
                validator.ValidateOrThrow(item, this.Schema);
            }
        }
    }
}