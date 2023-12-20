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
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A schema injector collection implementation.
    /// </summary>
    public class SchemaInjectorCollection : Dictionary<Type, ISchemaInjector>, ISchemaInjectorCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaInjectorCollection"/> class.
        /// </summary>
        /// <param name="serviceCollection">The service collection this set is tracking against.</param>
        public SchemaInjectorCollection(IServiceCollection serviceCollection)
        {
                this.ServiceCollection = Validation.ThrowIfNullOrReturn(serviceCollection, nameof(serviceCollection));
        }

        /// <inheritdoc />
        public IServiceCollection ServiceCollection { get; }
    }
}