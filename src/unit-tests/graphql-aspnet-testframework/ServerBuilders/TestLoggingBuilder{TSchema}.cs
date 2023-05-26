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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Tests.Framework.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A builder used to configure how logging will be injected into a created <see cref="TestServer{TSchema}"/>.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this component is targeting.</typeparam>
    public class TestLoggingBuilder<TSchema> : IGraphQLTestFrameworkComponent<TSchema>, ITestLoggingBuilder
        where TSchema : class, ISchema
    {
        private readonly List<ILoggerProvider> _customProviders;
        private LogLevel _minLevel;
        private bool _enableLogging;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLoggingBuilder{TSchema}"/> class.
        /// </summary>
        public TestLoggingBuilder()
        {
            _customProviders = new List<ILoggerProvider>();
            _minLevel = LogLevel.Warning;
            _enableLogging = true;
        }

        /// <inheritdoc />
        public virtual void Inject(IServiceCollection serviceCollection)
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

        /// <inheritdoc />
        public virtual ITestLoggingBuilder AddLoggerProvider(ILoggerProvider provider)
        {
            _customProviders.Add(provider);
            return this;
        }

        /// <inheritdoc />
        public virtual ITestLoggingBuilder SetMinimumLogLevel(LogLevel logLevel)
        {
            _minLevel = logLevel;
            return this;
        }

        /// <inheritdoc />
        public virtual ITestLoggingBuilder DisableLogging()
        {
            _enableLogging = false;
            return this;
        }
    }
}