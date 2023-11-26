// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Helper methods for configuring an <see cref="IServiceCollection"/>
    /// during startup.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds support for the execution of runtime field declarations (e.g. minimal api
        /// defined fields).
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddRuntimeFieldExecutionSupport(this IServiceCollection serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection.TryAddTransient<RuntimeFieldExecutionController>();
                serviceCollection.TryAddTransient<RuntimeExecutionDirective>();
            }

            return serviceCollection;
        }
    }
}