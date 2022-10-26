// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Converters
{
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Creates an appropriate json message converter for the given GraphqlWsLegacy message and client.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this factory creates converters for.</typeparam>
    public class GraphqlWsLegacyMessageConverterFactory<TSchema>
        where TSchema : class, ISchema
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyMessageConverterFactory{TSchema}" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to generate
        /// message converter instances.</param>
        public GraphqlWsLegacyMessageConverterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = Validation.ThrowIfNullOrReturn(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Creates an appropriate message converter to properly serialize the given GraphqlWsLegacy message
        /// in context of the provided client.
        /// </summary>
        /// <param name="message">The actual message for which to create a converter.</param>
        /// <returns>Returns a converter that can process the message as well as the concrete type
        /// of message that was used as a template to select the converter.</returns>
        public (JsonConverter, Type) CreateConverter(GraphqlWsLegacyMessage message)
        {
            JsonConverter converter = null;
            Type matchedType = typeof(GraphqlWsLegacyMessage);

            if (message != null)
            {
                ISchema schema;
                switch (message.Type)
                {
                    case GraphqlWsLegacyMessageType.DATA:
                        schema = _serviceProvider.GetService<TSchema>();
                        var writer = _serviceProvider.GetService<IGraphResponseWriter<TSchema>>();
                        converter = new GraphqlWsLegacyServerDataMessageConverter(schema, writer);
                        matchedType = typeof(GraphqlWsLegacyServerDataMessage);
                        break;

                    case GraphqlWsLegacyMessageType.COMPLETE:
                        converter = new GraphqlWsLegacyServerCompleteMessageConverter();
                        matchedType = typeof(GraphqlWsLegacyServerCompleteMessage);
                        break;

                    case GraphqlWsLegacyMessageType.ERROR:
                        schema = _serviceProvider.GetService<TSchema>();
                        converter = new GraphqlWsLegacyServerErrorMessageConverter(schema);
                        matchedType = typeof(GraphqlWsLegacyServerErrorMessage);
                        break;
                }
            }

            return (converter ?? new GraphqlWsLegacyMessageConverter(), matchedType);
        }
    }
}