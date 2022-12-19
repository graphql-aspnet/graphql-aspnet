// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Server level GraphQL settings that govern all schemas registered
    /// to this server instance. These values rarely need to be changed, howver; any changes should be made prior to
    /// calling <c>.AddGraphQL()</c> during startup.
    /// </summary>
    public static class GraphQLServerSettings
    {
        private static readonly object SETTINGS_LOCK = new object();
        private static ServiceLifetime _controllerServiceLifeTime;

        /// <summary>
        /// Initializes static members of the <see cref="GraphQLServerSettings"/> class.
        /// </summary>
        static GraphQLServerSettings()
        {
            _controllerServiceLifeTime = ServiceLifetime.Transient;
        }

        /// <summary>
        /// Gets or sets the <see cref="ServiceLifetime" /> under which all discovered
        /// <see cref="GraphController"/> and <see cref="GraphDirective"/> will be registered. (Default: Transient).
        /// </summary>
        /// <value>The controller service life time.</value>
        public static ServiceLifetime ControllerServiceLifeTime
        {
            get
            {
                lock (SETTINGS_LOCK)
                    return _controllerServiceLifeTime;
            }

            set
            {
                lock (SETTINGS_LOCK)
                    _controllerServiceLifeTime = value;
            }
        }
    }
}