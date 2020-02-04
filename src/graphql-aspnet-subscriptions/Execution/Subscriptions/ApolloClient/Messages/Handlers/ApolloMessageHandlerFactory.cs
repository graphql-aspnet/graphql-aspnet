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
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// Generates an appropriate handler for a given incoming message type.
    /// </summary>
    internal static class ApolloMessageHandlerFactory
    {
        /// <summary>
        /// Creates a handler that can process the message type that was recieved from a connected client.
        /// If unhandleable a default handler that will result in an error is returned.
        /// </summary>
        /// <typeparam name="TSchema">The type of the target schema to generate the handler for.</typeparam>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>IGraphQLOperationMessageHandler.</returns>
        public static IApolloMessageHandler CreateHandler<TSchema>(
            ApolloMessageType messageType)
            where TSchema : class, ISchema
        {
            switch (messageType)
            {
                case ApolloMessageType.CONNECTION_INIT:
                    return new ApolloConnectionInitHandler();

                case ApolloMessageType.START:
                    return new ApolloSubscriptionStartHandler<TSchema>();

                case ApolloMessageType.STOP:
                    return new ApolloSubscriptionStoppedHandler();

                case ApolloMessageType.CONNECTION_TERMINATE:
                    return new ApolloConnectionTerminateHandler();

                default:
                    return new ApolloUnknownMessageHandler();
            }
        }
    }
}