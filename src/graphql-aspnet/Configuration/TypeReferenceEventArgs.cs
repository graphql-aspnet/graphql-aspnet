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
        /// Initializes a new instance of the <see cref="TypeReferenceEventArgs" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="lifetime">The lifetime.</param>
        /// <param name="required">if set to <c>true</c> the type must be registered.</param>
        public TypeReferenceEventArgs(Type type, ServiceLifetime lifetime, bool required = false)
            : this(new ServiceDescriptor(type, type, lifetime), required)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeReferenceEventArgs"/> class.
        /// </summary>
        /// <param name="descriptor">The fully qualified service descriptor represneting the type
        /// to be added.</param>
        /// <param name="required">if set to <c>true</c> the type must be registered.</param>
        public TypeReferenceEventArgs(ServiceDescriptor descriptor, bool required = false)
        {
            this.Descriptor = descriptor;
            this.Required = required;
        }

        /// <summary>
        /// Gets a fully qualified descriptor that represents the type being added.
        /// </summary>
        /// <value>The descriptor.</value>
        public ServiceDescriptor Descriptor { get; }

        /// <summary>
        /// Gets a value indicating whether this type reference is required. A required reference MUST
        /// be registered to the service collection, an exception is thrown if it fails.
        /// </summary>
        /// <value><c>true</c> if required; otherwise, <c>false</c>.</value>
        public bool Required { get; }
    }
}