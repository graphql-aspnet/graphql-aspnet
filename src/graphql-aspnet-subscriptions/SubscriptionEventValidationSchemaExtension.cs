// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A server extension to validate all registered schema extensions as the
    /// server comes online.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to validate.</typeparam>
    public class SubscriptionEventValidationSchemaExtension<TSchema> : IGraphQLServerExtension
        where TSchema : class, ISchema
    {
        /// <inheritdoc />
        public void Configure(SchemaOptions options)
        {
            // no configuration required
        }

        /// <inheritdoc />
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
            var provider = app?.ApplicationServices ?? serviceProvider;
            if (provider == null)
                return;

            var scope = provider.CreateScope();
            var schema = scope.ServiceProvider.GetRequiredService<TSchema>();

            SchemaSubscriptionEventMap.EnsureSubscriptionEventsOrThrow(schema);
        }
    }
}