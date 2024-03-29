﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.Interfaces
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Schema;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// An interface defining a component for the test framework that adds
    /// additional functionality or logic into a test server instance as a whole package.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this component is targeting.</typeparam>
    public interface IGraphQLTestFrameworkComponent<TSchema> : IGraphQLTestFrameworkComponent
        where TSchema : class, ISchema
    {
        /// <summary>
        /// Allows this component to perform any common actions against the schema builder
        /// normally generated by calling <c>.AddGraphQL()</c>. This is typically used for registering
        /// middleware components to the test server.
        /// </summary>
        /// <param name="schemaBuilder">The schema builder.</param>
        public void Configure(ISchemaBuilder<TSchema> schemaBuilder)
        {
        }
    }
}