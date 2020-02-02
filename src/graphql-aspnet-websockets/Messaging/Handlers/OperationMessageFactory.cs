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
    using GraphQL.AspNet.Messaging.Messages;

    /// <summary>
    /// Generates an appropriate handler for a given incoming message type.
    /// </summary>
    internal static class OperationMessageFactory
    {
        /// <summary>
        /// Creates a handler that can process the message type requested. If unhandleable a default
        /// handler that will result in an error is returned.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>IGraphQLOperationMessageHandler.</returns>
        public static IGraphQLOperationMessageHandler CreateHandler(GraphQLOperationMessageType messageType)
        {
            switch (messageType)
            {
                case GraphQLOperationMessageType.CONNECTION_INIT:
                    return new ConnectionInitHandler();

                case GraphQLOperationMessageType.START:
                    return new OperationStartHandler();

                case GraphQLOperationMessageType.STOP:
                    return new OperationStoppedHandler();

                default:
                    return new UnknownMessageHandler();
            }
        }
    }
}