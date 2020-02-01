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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Event arguments related to a type being configured for a schema that needs to be added to a service collection.
    /// </summary>
    public class TypeReferenceEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReferenceEventArgs"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="lifetime">The lifetime.</param>
        public TypeReferenceEventArgs(Type type, ServiceLifetime lifetime)
        {
            this.Type = type;
            this.LifeTime = lifetime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReferenceEventArgs"/> class.
        /// </summary>
        /// <param name="descriptor">The fully qualified service descriptor represneting the type
        /// to be added.</param>
        public TypeReferenceEventArgs(ServiceDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }

        /// <summary>
        /// Gets the type reference that should be added to the DI container.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }

        /// <summary>
        /// Gets the service life time needed of the type.
        /// </summary>
        /// <value>The life time.</value>
        public ServiceLifetime LifeTime { get; }

        /// <summary>
        /// Gets a fully qualified descriptor that represents the type being added.
        /// </summary>
        /// <value>The descriptor.</value>
        public ServiceDescriptor Descriptor { get; }
    }
}