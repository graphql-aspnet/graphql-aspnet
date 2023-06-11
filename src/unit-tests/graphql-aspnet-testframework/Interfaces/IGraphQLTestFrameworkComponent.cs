// *************************************************************
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
    public interface IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// Allows this component to inject its DI components and perform any necessary registrations
        /// with the provided service collection such that it can function at runtime.
        /// </summary>
        /// <param name="serviceCollection">The service collection in which this
        /// component will inject its content.</param>
        public void Inject(IServiceCollection serviceCollection)
        {
        }

        /// <summary>
        /// Perform any configuration of the schema options before the schema is built.
        /// </summary>
        /// <param name="options">The schema options being constructed.</param>
        public void Configure(SchemaOptions options)
        {
        }
    }
}