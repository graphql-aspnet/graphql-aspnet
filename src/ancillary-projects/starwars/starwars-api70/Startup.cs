// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.StarWarsAPI7X
{
    using System;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.StarwarsAPI.Common.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.WebSockets;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        private const string ALL_ORIGINS_POLICY = "_allOrigins";

        private static readonly TimeSpan SOCKET_CONNECTION_KEEPALIVE = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration created to govern the
        /// application environment.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Configures the service collection to be built for this application instance.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<StarWarsDataRepository>();
            services.AddScoped<IStarWarsDataService, StarWarsDataService>();

            // apply an unrestricted cors policy for the demo services
            // to allow use on many of the tools for testing (graphiql, altair etc.)
            // Do not do this in production
            services.AddCors(options =>
            {
                options.AddPolicy(
                    ALL_ORIGINS_POLICY,
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            // ASP.NET websockets implementation must also be added to the runtime
            services.AddWebSockets((options) =>
            {
                // here add some common origins of various tools that may be
                // used for running this demo
                // do not add these in a production app unless you need
                // to
                options.AllowedOrigins.Add("http://localhost:5000");
                options.AllowedOrigins.Add("http://localhost:4000");
                options.AllowedOrigins.Add("http://localhost:3000");
                options.AllowedOrigins.Add("null");
                options.AllowedOrigins.Add("file://");
                options.AllowedOrigins.Add("ws://");

            });

            // ----------------------------------------------------------
            // Register GraphQL with the application
            // ----------------------------------------------------------
            // By default graphql will scan your assembly for any GraphControllers
            // and automatically wire them up to the schema
            // you can control which assemblies are scanned and which classes are registered using
            // the schema configuration options set here.
            //
            // in this example because of the two test projects (netcore3.1 and net5.0)
            // we have moved all the shared code to a common assembly (starwars-common) and are injecting it
            // as a single unit
            //
            // we then add subscription services to the schema builder returned from .AddGraphQL()
            services.AddGraphQL(options =>
            {
                options.ResponseOptions.ExposeExceptions = true;
                options.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Information;

                // options.ExecutionOptions.EnableMetrics = true;
                // options.ResponseOptions.ExposeMetrics = true;

                var assembly = typeof(StarWarsDataRepository).Assembly;
                options.AddAssembly(assembly);
            })
             .AddSubscriptions(options =>
             {
                 // this route path is set by default
                 // it is listed here just as a matter of example
                 options.Route = SubscriptionConstants.Routing.DEFAULT_SUBSCRIPTIONS_ROUTE;

                 // for some web based graphql tools such as graphiql and graphql-playground
                 // the default keep-alive timeout of 2 minutes is too long.
                 //
                 // still others (like graphql-playground running in electron) do not respond/configure
                 // for socket-level ping/pong frames to allow for socket-level keep alives
                 //
                 // here we set this demo project websocket keep-alive (at the server level)
                 // to be below all those thresholds to ensure a hassle free experience.
                 // In practice, you should configure your server (both subscription keep alives and socket keep alives)
                 // with an interval that is compatiable with your client side environment.
                 options.ConnectionKeepAliveInterval = SOCKET_CONNECTION_KEEPALIVE;
             });

            // if you have rest controllers this item be sure they are included.
            // Graphql and rest can live side by side in the same project without issue
            // --------------------------------------------------
            // builder.Services.AddControllers();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The asp.net application builder.</param>
        /// <param name="env">The configured host environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.AddStarWarsStartedMessageToConsole();

            app.UseRouting();

            app.UseCors(ALL_ORIGINS_POLICY);

            app.UseAuthorization();

            // enable web sockets on this server instance
            // this must be done before a call to 'UseGraphQL' if subscriptions are enabled for any
            // schema otherwise the subscriptions may not register correctly
            app.UseWebSockets();

            // if you have no rest controllers this item can be safely skipped
            // graphql and rest can live side by side in the same project without issue
            // app.UseEndpoints(endpoints =>
            // {
            //    endpoints.MapControllers();
            // });

            // ************************************************************
            // Finalize the graphql setup:
            // 1) Loading the schema
            // 2) Publish the route to hook the graphql runtime to ASP.NET.
            //
            // Be sure to register it after "UseAuthorization" if you need it.
            //
            // If the construction of your runtime schema has any errors they will be thrown here
            // before your application starts listening for requests.
            // ************************************************************
            app.UseGraphQL();
        }

        /// <summary>
        /// Gets the environment configuration for this instance.
        /// </summary>
        /// <value>The configuration item.</value>
        public IConfiguration Configuration { get; }
    }
}