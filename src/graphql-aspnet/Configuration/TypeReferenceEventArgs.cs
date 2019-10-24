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
        /// Gets the type reference that should be added to the DI container.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }

        /// <summary>
        /// Gets the service life time needed of the type.
        /// </summary>
        /// <value>The life time.</value>
        public ServiceLifetime LifeTime { get; }
    }
}