// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Converters
{
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.ServerMessages;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Creates an appropriate json message converter for the given graphql-ws message and client.
    /// </summary>
    internal class GQLWSMessageConverterFactory
    {
        /// <summary>
        /// Creates an appropriate message converter to properly serialize the given graphql-ws message
        /// in context of the provided client.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the message was created for.</typeparam>
        /// <param name="client">The client to recieve the message. The factory may use
        /// some client specific meta data as input parameters to the converters to customize the
        /// message serialization routine for a specific schema or message type.</param>
        /// <param name="message">The actual message for which to create a converter.</param>
        /// <returns>Returns a converter that can process the message as well as the concrete type
        /// of message that was used as a template to select the converter.</returns>
        public (JsonConverter, Type) CreateConverter<TSchema>(ISubscriptionClientProxy client, GQLWSMessage message)
            where TSchema : class, ISchema
        {
            JsonConverter converter = null;
            Type matchedType = typeof(GQLWSMessage);

            ISchema schema = null;
            if (message != null)
            {
                switch (message.Type)
                {
                    case GQLWSMessageType.NEXT:
                        schema = client.ServiceProvider.GetService<TSchema>();
                        var writer = client.ServiceProvider.GetService<IGraphResponseWriter<TSchema>>();
                        converter = new GQLWSServerDataMessageConverter(schema, writer);
                        matchedType = typeof(GQLWSServerNextDataMessage);
                        break;

                    case GQLWSMessageType.COMPLETE:
                        converter = new GQLWSServerCompleteMessageConverter();
                        matchedType = typeof(GQLWSSubscriptionCompleteMessage);
                        break;

                    case GQLWSMessageType.ERROR:
                        schema = client.ServiceProvider.GetService<TSchema>();
                        converter = new GQLWSServerErrorMessageConverter(schema);
                        matchedType = typeof(GQLWSServerErrorMessage);
                        break;
                }
            }

            return (converter ?? new GQLWSMessageConverter(), matchedType);
        }
    }
}