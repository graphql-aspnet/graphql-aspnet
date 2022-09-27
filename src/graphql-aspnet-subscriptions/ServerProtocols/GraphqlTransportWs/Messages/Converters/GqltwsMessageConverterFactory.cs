// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Converters
{
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Creates an appropriate json message converter for the given graphql-ws message and client.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema to convert for.</typeparam>
    internal class GqltwsMessageConverterFactory<TSchema>
        where TSchema : class, ISchema
    {
        private readonly GqltwsClientProxy<TSchema> _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsMessageConverterFactory{TSchema}"/> class.
        /// </summary>
        /// <param name="client">The specific client this factory makes converters for.</param>
        public GqltwsMessageConverterFactory(GqltwsClientProxy<TSchema> client)
        {
            _client = Validation.ThrowIfNullOrReturn(client, nameof(client));
        }

        /// <summary>
        /// Creates an appropriate message converter to properly serialize the given graphql-ws message
        /// in context of the provided client.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being converted.</typeparam>
        /// <param name="message">The actual message for which to create a converter.</param>
        /// <returns>Returns a converter that can process the message as well as the concrete type
        /// of message that was used as a template to select the converter.</returns>
        public (JsonConverter, Type) CreateConverter<TMessage>(TMessage message)
            where TMessage : class
        {
            JsonConverter converter = null;
            Type matchedType = typeof(GqltwsMessage);

            var gqlMessage = message as GqltwsMessage;

            ISchema schema = null;
            if (gqlMessage != null)
            {
                switch (gqlMessage.Type)
                {
                    case GqltwsMessageType.NEXT:
                        schema = _client.ClientConnection.ServiceProvider.GetService<TSchema>();
                        var writer = _client.ClientConnection.ServiceProvider.GetService<IGraphResponseWriter<TSchema>>();

                        converter = new GqltwsServerDataMessageConverter(schema, writer);
                        matchedType = typeof(GqltwsServerNextDataMessage);
                        break;

                    case GqltwsMessageType.COMPLETE:
                        converter = new GqltwsServerCompleteMessageConverter();
                        matchedType = typeof(GqltwsSubscriptionCompleteMessage);
                        break;

                    case GqltwsMessageType.ERROR:
                        schema = _client.ClientConnection.ServiceProvider.GetService<TSchema>();
                        converter = new GqltwsServerErrorMessageConverter(schema);
                        matchedType = typeof(GqltwsServerErrorMessage);
                        break;
                }
            }

            return (converter ?? new GqltwsMessageConverter(), matchedType);
        }
    }
}