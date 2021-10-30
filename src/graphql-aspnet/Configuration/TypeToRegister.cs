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
    using System;
    using GraphQL.AspNet.Common;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A type gathered by a <see cref="SchemaOptions"/> setup that needs to be registered
    /// to a DI container.
    /// </summary>
    public class TypeToRegister
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeToRegister" /> class.
        /// </summary>
        /// <param name="type">The type to register with DI.</param>
        /// <param name="lifeTime">The lifetime scope of the type.</param>
        /// <param name="required">if set to <c>true</c> the type must be registered. Failure to
        /// register the type will result in an exception.</param>
        public TypeToRegister(Type type, ServiceLifetime lifeTime, bool required = false)
            : this(type, type, lifeTime, required)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeToRegister" /> class.
        /// </summary>
        /// <param name="serviceType">The type to expose <paramref name="implementationType"/> when
        /// requested through the DI container.</param>
        /// <param name="implementationType">The concrete implementation type
        /// used to fulfill a request.</param>
        /// <param name="lifeTime">The lifetime scope of the type.</param>
        /// <param name="required">if set to <c>true</c> the type must be registered. Failure to
        /// register the type will result in an exception.</param>
        public TypeToRegister(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifeTime,
            bool required = false)
        {
            this.ServiceType = Validation.ThrowIfNullOrReturn(serviceType, nameof(serviceType));
            this.ImplementationType = Validation.ThrowIfNullOrReturn(implementationType, nameof(implementationType));
            this.ServiceLifeTime = lifeTime;
            this.Required = required;
        }

        /// <summary>
        /// Creates a new, fully qualified service descriptor used to register
        /// this type instance into the DI container.
        /// </summary>
        /// <returns>ServiceDescriptor.</returns>
        public ServiceDescriptor CreateServiceDescriptor()
        {
            return new ServiceDescriptor(this.ServiceType, this.ImplementationType, this.ServiceLifeTime);
        }

        /// <summary>
        /// Gets a value indicating whether this type reference is required. A required reference MUST
        /// be registered to the service collection, an exception is thrown if it fails.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required { get; }

        /// <summary>
        /// Gets the type to serve <see cref="ImplementationType"/> as when requested.
        /// through DI.
        /// </summary>
        /// <value>The type of the service.</value>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the concrete implementation type used to serve <see cref="ServiceType"/>.
        /// </summary>
        /// <value>The type of the implementation.</value>
        public Type ImplementationType { get; }

        /// <summary>
        /// Gets the lifetime scope to register this type as.
        /// </summary>
        /// <value>The service life time.</value>
        public ServiceLifetime ServiceLifeTime { get; }
    }
}