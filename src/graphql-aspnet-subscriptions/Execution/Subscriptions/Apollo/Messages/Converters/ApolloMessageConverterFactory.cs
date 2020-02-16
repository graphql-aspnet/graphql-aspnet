// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Converters
{
    using System;
    using System.Text.Json.Serialization;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.Common;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using Microsoft.Extensions.DependencyInjection;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo.Messages.ServerMessages;

    /// <summary>
    /// Creates an appropriate json message converter for the given apollo message and client.
    /// </summary>
    public static class ApolloMessageConverterFactory
    {
        /// <summary>
        /// Creates the appropriate message converter.
        /// </summary>
        /// <typeparam name="TSchema">The type of the schema the message was created for.</typeparam>
        /// <param name="client">The client.</param>
        /// <param name="message">The message.</param>
        /// <returns>System.ValueTuple&lt;JsonConverter, Type&gt;.</returns>
        public static (JsonConverter, Type) CreateConverter<TSchema>(ISubscriptionClientProxy client, ApolloMessage message)
            where TSchema : class, ISchema
        {
            JsonConverter converter = null;
            Type matchedType = typeof(ApolloMessage);
            if (message != null)
            {
                switch (message.Type)
                {
                    case ApolloMessageType.DATA:
                        var schema = client.ServiceProvider.GetService<TSchema>();
                        var writer = client.ServiceProvider.GetService<IGraphResponseWriter<TSchema>>();
                        converter = new ApolloServerDataMessageConverter(schema, writer);
                        matchedType = typeof(ApolloServerDataMessage);
                        break;
                }
            }

            return (converter ?? new ApolloMessageConverter(), matchedType);
        }
    }
}