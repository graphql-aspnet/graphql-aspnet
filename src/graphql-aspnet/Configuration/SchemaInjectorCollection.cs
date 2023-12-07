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
    using GraphQL.AspNet.Interfaces.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A schema injector collection implementation.
    /// </summary>
    public class SchemaInjectorCollection : Dictionary<Type, ISchemaInjector>, ISchemaInjectorCollection
    {
        /// <inheritdoc />
        public IServiceCollection ServiceCollection { get; set; }
    }
}