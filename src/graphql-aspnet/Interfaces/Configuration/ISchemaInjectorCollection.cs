// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An interface used by the injector to track multiple schema injectors
    /// used for injecting.
    /// </summary>
    public interface ISchemaInjectorCollection : IDictionary<Type, ISchemaInjector>
    {
        /// <summary>
        /// Gets the service collection to which this collection is attached.
        /// </summary>
        /// <value>The attached service collection.</value>
        public IServiceCollection ServiceCollection { get; }
    }
}