// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Helper methods for dependency injection.
    /// </summary>
    public static class DiExtensions
    {
        /// <summary>
        /// Replace the first instance one service registration with a new implementation and/or lifetime scope.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="serviceDescriptor">The service descriptor.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddOrUpdate(this IServiceCollection serviceCollection, ServiceDescriptor serviceDescriptor)
        {
            if (serviceCollection.Contains(serviceDescriptor))
                serviceCollection.Replace(serviceDescriptor);
            else
                serviceCollection.Add(serviceDescriptor);

            return serviceCollection;
        }

        /// <summary>
        /// Replace the first instance one service registration with a new implementation and/or lifetime scope.
        /// </summary>
        /// <typeparam name="TService">The type of the service to register for.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation to provide.</typeparam>
        /// <param name="services">The services.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection Replace<TService, TImplementation>(
            this IServiceCollection services,
            ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
            if (descriptorToRemove == null)
                throw new ArgumentOutOfRangeException($"The service type {typeof(TService).FriendlyName()} does not exist in the service collection, replace fails.");

            services.Remove(descriptorToRemove);
            var descriptorToAdd = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
            services.Add(descriptorToAdd);
            return services;
        }

        /// <summary>
        /// Replace one service registration with a new implementation and/or lifetime scope.
        /// </summary>
        /// <typeparam name="TService">The type of the t service.</typeparam>
        /// <param name="services">The services.</param>
        /// <param name="implementationFactory">The implementation factory.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection Replace<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory,
            ServiceLifetime lifetime)
            where TService : class
        {
            var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
            if (descriptorToRemove == null)
                throw new ArgumentOutOfRangeException($"The service type {typeof(TService).FriendlyName()} does not exist in the service collection, replace fails.");

            services.Remove(descriptorToRemove);
            var descriptorToAdd = new ServiceDescriptor(typeof(TService), implementationFactory, lifetime);
            services.Add(descriptorToAdd);
            return services;
        }
    }
}