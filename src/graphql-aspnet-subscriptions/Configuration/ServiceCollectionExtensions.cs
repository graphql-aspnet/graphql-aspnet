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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Helpful extension methods for the <see cref="IServiceCollection"/>
    /// when dealing with subscriptions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds support for the execution of runtime subscription field declarations (e.g. minimal api
        /// defined fields).
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddSubscriptionRuntimeFieldExecutionSupport(this IServiceCollection serviceCollection)
        {
            if (serviceCollection != null)
            {
                serviceCollection.TryAddTransient<SubscriptionEnabledRuntimeFieldExecutionController>();
            }

            return serviceCollection;
        }
    }
}