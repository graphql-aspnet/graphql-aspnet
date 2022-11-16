// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using GraphQL.AspNet.Interfaces.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="IGraphQLGlobalConfiguration" />
    public class DefaultGraphQLGLobalConfiguration : IGraphQLGlobalConfiguration
    {
        /// <inheritdoc />
        public ServiceLifetime ControllerServiceLifeTime { get; set; } = ServiceLifetime.Transient;
    }
}