// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Mvc
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A set of extensions to configure web socket support at startup.
    /// </summary>
    public static class GraphQLMvcSchemaWebSocketBuilderExtensions
    {
        /// <summary>
        /// Configures and injects web socket support for GraphQL ASP.NET enabling subscriptions
        /// on this server.  This method will also attempt to call "UseWebSockets" and assign all
        /// applicable configuration options.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="enableSubscriptions">if set to <c>true</c> websocket support will be
        /// added to the server and subscription support will be enabled for all
        /// schemas.</param>
        public static void UseGraphQL(this IApplicationBuilder app, bool enableSubscriptions = false)
        {
            if (enableSubscriptions)
            {
                app.UseWebSockets();
            }

            app.ApplicationServices.UseGraphQL();
        }
    }
}