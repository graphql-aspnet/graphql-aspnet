// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.ServerBuilders
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A builder used to configure how logging will be injected into a created <see cref="TestServer{TSchema}"/>.
    /// </summary>
    public class TestLoggingBuilder : IGraphTestFrameworkComponent
    {
        private readonly List<ILoggerProvider> _customProviders;
        private LogLevel _minLevel;
        private bool _enableLogging;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLoggingBuilder"/> class.
        /// </summary>
        public TestLoggingBuilder()
        {
            _customProviders = new List<ILoggerProvider>();
            _minLevel = LogLevel.Warning;
            _enableLogging = true;
        }

        /// <summary>
        /// Injects the component configured by this builder with a service collection instance.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public void Inject(IServiceCollection serviceCollection)
        {
            if (_enableLogging)
            {
                serviceCollection.AddLogging((options) =>
                {
                    foreach (var provider in _customProviders)
                        options.AddProvider(provider);

                    options.SetMinimumLevel(_minLevel);
                });
            }
        }

        /// <summary>
        /// Adds a logger provider that will be injected as part of the logging settings when this
        /// component is registered to a service collection.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns>TestLoggingBuilder.</returns>
        public TestLoggingBuilder AddLoggerProvider(ILoggerProvider provider)
        {
            _customProviders.Add(provider);
            return this;
        }

        /// <summary>
        /// Sets the global minimum level of any log message for it to be "logged".
        /// </summary>
        /// <param name="logLevel">The log level to value.</param>
        /// <returns>TestLoggingBuilder.</returns>
        public TestLoggingBuilder SetMinimumLogLevel(LogLevel logLevel)
        {
            _minLevel = logLevel;
            return this;
        }

        /// <summary>
        /// Disables all logging services. It will not be injected into the service collection
        /// when the test server starts.
        /// </summary>
        /// <returns>TestLoggingBuilder.</returns>
        public TestLoggingBuilder DisableLogging()
        {
            _enableLogging = false;
            return this;
        }
    }
}