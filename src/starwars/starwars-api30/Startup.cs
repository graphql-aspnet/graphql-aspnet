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
    using GraphQL.AspNet.Configuration;
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

            // ----------------------------------------------------------
            // Register GraphQL with the application
            // ----------------------------------------------------------
            // By default graphql will scan your assembly for any GraphControllers
            // and automatically wire them up to the schema
            // you can control which assemblies are scanned and which classes are registered using
            // the schema configuration options set here.
            //
            // in this example because of the two test projects netcore2.2 and netcore3.0
            // we have moved all the shared code to a common assembly (starwars-common) and are injecting it
            // as a single unit
            services.AddGraphQL(options =>
             {
                 options.ResponseOptions.ExposeExceptions = true;
                 options.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Information;

                 var assembly = typeof(StarWarsDataRepository).Assembly;
                 options.AddGraphAssembly(assembly);
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

            app.UseAuthorization();

            // ************************************************************
            // Finalize the graphql setup by load the schema, build out the templates for all found graph types
            // and publish the route to hook the graphql runtime to the web.
            // be sure to register it after "UseAuthorization" if you require access to this.User
            // ************************************************************
            app.UseGraphQL();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}