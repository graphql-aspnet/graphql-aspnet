// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A server extension to validate any registered subscription extensions as the
    /// server comes online.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to validate.</typeparam>
    public sealed class SubscriptionEventValidationSchemaExtension<TSchema> : IGraphQLServerExtension
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

            SubscriptionEventSchemaMap.EnsureSubscriptionEventsOrThrow(schema);
        }
    }
}