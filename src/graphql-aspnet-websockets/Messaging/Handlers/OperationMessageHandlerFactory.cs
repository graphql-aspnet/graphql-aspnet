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

    /// <summary>
    /// Generates an appropriate handler for a given incoming message type.
    /// </summary>
    internal static class OperationMessageHandlerFactory
    {
        /// <summary>
        /// Creates a handler that can process the message type requested. If unhandleable a default
        /// handler that will result in an error is returned.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>IGraphQLOperationMessageHandler.</returns>
        public static IGraphQLOperationMessageHandler CreateHandler(OperationMessageType messageType)
        {
            switch (messageType)
            {
                case OperationMessageType.GQL_CONNECTION_INIT:
                    return null;
            }

            return null;
        }
    }
}