// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging.Handlers
{
    using System.Collections;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Messaging.Messages;
    using GraphQL.AspNet.Middleware.QueryExecution;

    /// <summary>
    /// Generates an appropriate handler for a given incoming message type.
    /// </summary>
    internal static class OperationMessageFactory
    {
        /// <summary>
        /// Creates a handler that can process the message type requested. If unhandleable a default
        /// handler that will result in an error is returned.
        /// </summary>
        /// <typeparam name="TSchema">The type of the target schema for this handler.</typeparam>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>IGraphQLOperationMessageHandler.</returns>
        public static IGraphQLOperationMessageHandler CreateHandler<TSchema>(
            ApolloMessageType messageType)
            where TSchema : class, ISchema
        {
            switch (messageType)
            {
                case ApolloMessageType.CONNECTION_INIT:
                    return new ConnectionInitHandler();

                case ApolloMessageType.START:
                    return new OperationStartHandler<TSchema>();

                case ApolloMessageType.STOP:
                    return new OperationStoppedHandler();

                default:
                    return new UnknownMessageHandler();
            }
        }
    }
}