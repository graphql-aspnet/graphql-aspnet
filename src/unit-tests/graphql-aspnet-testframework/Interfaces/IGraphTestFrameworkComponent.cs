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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Allows for the creation of new test framework builders for defining (and injecting) additional
    /// test functionality into a test server instance.
    /// </summary>
    public interface IGraphTestFrameworkComponent
    {
        /// <summary>
        /// Injects the component configured by this builder with a service collection instance.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        void Inject(IServiceCollection serviceCollection);
    }
}