// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerExtensions.MultipartRequests
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine.TypeMakers;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Schema;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// A server extension that configures support for the <c>graphql-multipart-request</c> specification on the
    /// given schema.
    /// </summary>
    /// <remarks>See: <see href="https://github.com/jaydenseric/graphql-multipart-request-spec" /> for specification details and a list of compatiable clients.</remarks>
    public class MultipartRequestServerExtension : IGraphQLServerExtension
    {
        private readonly Action<MultipartRequestConfiguration> _configAction;

        private Type _expectedProcessorType;
        private MultipartRequestConfiguration _config;
        private Type _schemaType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestServerExtension" /> class.
        /// </summary>
        public MultipartRequestServerExtension()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestServerExtension" /> class.
        /// </summary>
        /// <param name="configureAction">An action that, when executed, will configure an object
        /// containing all the different options this extension supports.</param>
        public MultipartRequestServerExtension(Action<MultipartRequestConfiguration> configureAction = null)
        {
            _configAction = configureAction;
        }

        /// <inheritdoc />
        public virtual void Configure(SchemaOptions options)
        {
            _schemaType = options.SchemaType;
            var configType = typeof(MultipartRequestConfiguration<>).MakeGenericType(_schemaType);
            _config = InstanceFactory.CreateInstance(configType) as MultipartRequestConfiguration;

            if (_config == null)
            {
                throw new SchemaConfigurationException(
                    $"Unable to create a configuration object for the {nameof(MultipartRequestServerExtension)} " +
                    $"(Target Schema: {options.SchemaType.Name}");
            }

            if (_configAction != null)
                _configAction(_config);

            _expectedProcessorType = typeof(MultipartRequestGraphQLHttpProcessor<>).MakeGenericType(_schemaType);

            if (_config.RegisterMultipartRequestHttpProcessor)
            {
                if (options.QueryHandler.HttpProcessorType != null)
                {
                    throw new SchemaConfigurationException(
                        $"The {nameof(MultipartRequestServerExtension)} must register " +
                        $"a custom {nameof(options.QueryHandler.HttpProcessorType)} to add support for " +
                        $"batch operations and file upload processing. A custom {nameof(options.QueryHandler.HttpProcessorType)} " +
                        $"named {options.QueryHandler.HttpProcessorType.FriendlyName()} has already been defined. This extension " +
                        $"cannot be registered for the '{_schemaType.FriendlyName()}' schema.");
                }

                options.QueryHandler.HttpProcessorType = _expectedProcessorType;
            }

            // register a scalar that represents the file
            var isRegisteredScalar = GraphQLProviders.ScalarProvider.IsScalar(typeof(FileUpload));
            if (!isRegisteredScalar)
            {
                GraphQLProviders.ScalarProvider.RegisterCustomScalar(typeof(FileUploadScalarGraphType));
            }

            // register the config options for the schema
            var configurationServiceType = typeof(IMultipartRequestConfiguration<>).MakeGenericType(options.SchemaType);
            options.ServiceCollection.TryAdd(new ServiceDescriptor(configurationServiceType, _config));

            // register the http context parser for the schema
            var payloadParserServiceType = typeof(IMultiPartHttpFormPayloadParser<>).MakeGenericType(options.SchemaType);
            var payloadParserImplementationType = typeof(MultiPartHttpFormPayloadParser<>).MakeGenericType(options.SchemaType);
            options.ServiceCollection.TryAdd(new ServiceDescriptor(payloadParserServiceType, payloadParserImplementationType, ServiceLifetime.Transient));

            // perform the rest of the DI registrations
            options.ServiceCollection.TryAddSingleton<IFileUploadScalarValueMaker, DefaultFileUploadScalarValueMaker>();
        }

        /// <inheritdoc />
        public virtual void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
            if (serviceProvider == null)
                return;

            if (_config.RequireMultipartRequestHttpProcessor)
            {
                // the developer or another extension could have changed out the registered processor type.
                // Validate that the registered processor for this schema is the expected one
                // and raise an exception if its not.
                var expectedInterface = typeof(IGraphQLHttpProcessor<>).MakeGenericType(_schemaType);

                using var scope = serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetService(expectedInterface);
                if (processor == null || !Validation.IsCastable(processor.GetType(), _expectedProcessorType))
                {
                    throw new SchemaConfigurationException(
                        $"The {nameof(SchemaQueryHandlerConfiguration.HttpProcessorType)} registered by the " +
                        $"{nameof(MultipartRequestServerExtension)} " +
                        $"was removed or replaced during the configuration of schema '{_schemaType.FriendlyName()}'. " +
                        $"Extension initialization cannot be completed.");
                }
            }
        }
    }
}