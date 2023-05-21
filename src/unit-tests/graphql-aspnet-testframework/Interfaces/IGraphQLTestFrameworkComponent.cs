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
    public interface IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// Instructs this component to inject its contents and perform any necessary registrations
        /// with the provided service collection such that it can function at runtim.
        /// </summary>
        /// <param name="serviceCollection">The service collection in which this
        /// component will inject its content.</param>
        void Inject(IServiceCollection serviceCollection);
    }
}