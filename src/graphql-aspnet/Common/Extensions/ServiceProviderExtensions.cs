// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using GraphQL.AspNet.Interfaces.Logging;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions for interacting with an <see cref="IServiceProvider"/>.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Writes the log entry to an event logger if it can be generated from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to extract a logger from.</param>
        /// <param name="writeFunction">A function that would create and write a log entry.</param>
        public static void WriteLogEntry(this IServiceProvider serviceProvider, Action<IGraphEventLogger> writeFunction)
        {
            if (serviceProvider != null)
            {
                using (var scopedProvider = serviceProvider.CreateScope())
                {
                    var logger = scopedProvider.ServiceProvider.GetService<IGraphEventLogger>();
                    if (logger != null)
                        writeFunction(logger);
                }
            }
        }
    }
}