// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.Server
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Configuration.Mvc;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.WebSockets;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>Task.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLogging();
            builder.Services.AddWebSockets((options) => { });
            builder.Services.AddControllers();

            var alerts = new SubscriptionClientDispatchQueueAlertSettings();
            alerts.AddThreshold(
                LogLevel.Debug,
                1000,
                TimeSpan.FromSeconds(30));

            alerts.AddThreshold(
                LogLevel.Information,
                5000,
                TimeSpan.FromSeconds(60));

            alerts.AddThreshold(
                LogLevel.Warning,
                10_000,
                TimeSpan.FromSeconds(120));

            alerts.AddThreshold(
                LogLevel.Critical,
                100_000,
                TimeSpan.FromSeconds(15));

            builder.Services.AddSingleton<ISubscriptionClientDispatchQueueAlertSettings>(alerts);

            SubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount = 5000;

            // Add services to the container.
            // builder.Services.AddSingleton<Repository>();
            builder.Services
                .AddGraphQL()
                .AddSubscriptions(options =>
                {
                    options.ConnectionKeepAliveInterval = TimeSpan.FromSeconds(5);
                });

            var app = builder.Build();

            app.UseWebSockets();

            app.UseRouting();

            app.UseGraphQL();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}