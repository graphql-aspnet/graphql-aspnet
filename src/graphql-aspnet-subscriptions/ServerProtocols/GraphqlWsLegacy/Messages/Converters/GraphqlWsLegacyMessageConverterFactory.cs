// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.GraphqlWsLegacy.Messages.Converters
{
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.Common;
    using GraphQL.AspNet.GraphqlWsLegacy.Messages.ServerMessages;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Creates an appropriate json message converter for the given GraphqlWsLegacy message and client.
    /// </summary>
    public class GraphqlWsLegacyMessageConverterFactory
    {
        /// <summary>
        /// Creates an appropriate message converter to properly serialize the given GraphqlWsLegacy message
        /// in context of the provided client.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the message was created for.</typeparam>
        /// <param name="client">The client to recieve the message. The factory may use
        /// some client specific meta data as input parameters to the converters to customize the
        /// message serialization routine for a specific schema or message type.</param>
        /// <param name="message">The actual message for which to create a converter.</param>
        /// <returns>Returns a converter that can process the message as well as the concrete type
        /// of message that was used as a template to select the converter.</returns>
        public (JsonConverter, Type) CreateConverter<TSchema>(ISubscriptionClientProxy client, GraphqlWsLegacyMessage message)
            where TSchema : class, ISchema
        {
            JsonConverter converter = null;
            Type matchedType = typeof(GraphqlWsLegacyMessage);

            ISchema schema = null;
            if (message != null)
            {
                switch (message.Type)
                {
                    case GraphqlWsLegacyMessageType.DATA:
                        schema = client.ServiceProvider.GetService<TSchema>();
                        var writer = client.ServiceProvider.GetService<IGraphResponseWriter<TSchema>>();
                        converter = new GraphqlWsLegacyServerDataMessageConverter(schema, writer);
                        matchedType = typeof(GraphqlWsLegacyServerDataMessage);
                        break;

                    case GraphqlWsLegacyMessageType.COMPLETE:
                        converter = new GraphqlWsLegacyServerCompleteMessageConverter();
                        matchedType = typeof(GraphqlWsLegacyServerCompleteMessage);
                        break;

                    case GraphqlWsLegacyMessageType.ERROR:
                        schema = client.ServiceProvider.GetService<TSchema>();
                        converter = new GraphqlWsLegacyServerErrorMessageConverter(schema);
                        matchedType = typeof(GraphqlWsLegacyServerErrorMessage);
                        break;
                }
            }

            return (converter ?? new GraphqlWsLegacyMessageConverter(), matchedType);
        }
    }
}