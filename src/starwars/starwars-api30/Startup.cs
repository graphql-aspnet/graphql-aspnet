// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.StarWarsAPI30
{
    using System;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.StarwarsAPI.Common.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        private const string ALL_ORIGINS_POLICY = "_allOrigins";

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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

            // ----------------------------------------------------------
            // Register GraphQL with the application
            // ----------------------------------------------------------
            // By default graphql will scan your assembly for any GraphControllers
            // and automatically wire them up to the schema
            // you can control which assemblies are scanned and which classes are registered using
            // the schema configuration options set here.
            //
            // in this example because of the two test projects (netcore2.2 and netcore3.0)
            // we have moved all the shared code to a common assembly (starwars-common) and are injecting it
            // as a single unit
            services.AddGraphQL(options =>
             {
                 options.ResponseOptions.ExposeExceptions = true;
                 options.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Information;

                 var assembly = typeof(StarWarsDataRepository).Assembly;
                 options.AddGraphAssembly(assembly);
             })
            .AddSubscriptions(options =>
            {
                // this route path is set by default
                // it is listed here just as a matter of example
                options.Route = SubscriptionConstants.Routing.DEFAULT_SUBSCRIPTIONS_ROUTE;
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.AddStarWarsStartedMessageToConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(ALL_ORIGINS_POLICY);

            app.UseAuthorization();

            // enable web sockets on this server instance
            // this must be done before graphql if subscriptions are enabled for any
            // schema otherwise the subscriptions may not register correctly
            app.UseWebSockets(new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // ************************************************************
            // Finalize the graphql setup by load the schema, build out the templates for all found graph types
            // and publish the route to hook the graphql runtime to the web.
            // be sure to register it after "UseAuthorization" if you require access to this.User
            // ************************************************************
            app.UseGraphQL();
        }
    }
}