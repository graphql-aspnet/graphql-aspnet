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
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A framework component used to determine how logging will support will be
    /// added to the test server.
    /// </summary>
    public interface ITestLoggingBuilder : IGraphQLTestFrameworkComponent
    {
        /// <summary>
        /// Adds a logger provider that will be injected as part of the logging settings when this
        /// component is registered to a service collection.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>TestLoggingBuilder.</returns>
        public ITestLoggingBuilder AddLoggerProvider(ILoggerProvider provider);

        /// <summary>
        /// Sets the global minimum level of any log message for it to be "logged".
        /// </summary>
        /// <param name="logLevel">The log level to value.</param>
        /// <returns>TestLoggingBuilder.</returns>
        public ITestLoggingBuilder SetMinimumLogLevel(LogLevel logLevel);

        /// <summary>
        /// Disables all logging services. It will not be injected into the service collection
        /// when the test server starts.
        /// </summary>
        /// <returns>TestLoggingBuilder.</returns>
        public ITestLoggingBuilder DisableLogging();
    }
}