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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A configuration that determines operations that effect all registered schemas
    /// for the entire server instance.
    /// </summary>
    public interface IGraphQLGlobalConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="ServiceLifetime" /> under which all <see cref="GraphController"/> and
        /// <see cref="GraphDirective"/> will be registered. (Default: Transient).
        /// </summary>
        /// <value>The controller service life time.</value>
        ServiceLifetime ControllerServiceLifeTime { get; set; }
    }
}