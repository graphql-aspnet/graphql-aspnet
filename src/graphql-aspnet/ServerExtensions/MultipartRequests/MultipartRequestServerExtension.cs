﻿// *************************************************************
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
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Exceptions;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Web;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Schema;
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
        private readonly bool _registerCustomProcessor;
        private Type _expectedProcessorType;
        private Type _schemaType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestServerExtension"/> class.
        /// </summary>
        public MultipartRequestServerExtension()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipartRequestServerExtension"/> class.
        /// </summary>
        /// <param name="registerCustomProcessor">If set to <c>true</c> the extension will register
        /// its own custom http query processor to handle incoming queries.  When <c>false</c>,
        /// no custom processor will be registerd and you must handle query parsing and file mapping
        /// in some other manner.</param>
        public MultipartRequestServerExtension(bool registerCustomProcessor = true)
        {
            _registerCustomProcessor = registerCustomProcessor;
        }

        /// <inheritdoc />
        public void Configure(SchemaOptions options)
        {
            if (_registerCustomProcessor)
            {
                if (options.QueryHandler.HttpProcessorType != null)
                {
                    throw new SchemaConfigurationException(
                        $"The {nameof(MultipartRequestServerExtension)} must register " +
                        $"a custom {nameof(options.QueryHandler.HttpProcessorType)} to add support for " +
                        $"batch operations and file upload processing. A custom {nameof(options.QueryHandler.HttpProcessorType)} " +
                        $"named {options.QueryHandler.HttpProcessorType.FriendlyName()} has already been defined. This extension " +
                        $"cannot be registered for the '{options.SchemaType.FriendlyName()}' schema.");
                }

                _schemaType = options.SchemaType;

                _expectedProcessorType = typeof(MultipartRequestGraphQLHttpProcessor<>).MakeGenericType(_schemaType);
                options.QueryHandler.HttpProcessorType = _expectedProcessorType;
            }

            // register a scalar that represents the file
            GraphQLProviders.ScalarProvider.RegisterCustomScalar(typeof(FileUploadScalarGraphType));

            // perform the rest of the DI registrations
            options.ServiceCollection.TryAddSingleton<IFileUploadScalarValueMaker, DefaultFileUploadScalarValueMaker>();
        }

        /// <inheritdoc />
        public void UseExtension(IApplicationBuilder app = null, IServiceProvider serviceProvider = null)
        {
            if (serviceProvider == null)
                return;

            if (_registerCustomProcessor)
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