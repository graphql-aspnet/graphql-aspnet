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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Messaging.ServerMessages;

    /// <summary>
    /// A handler for processing the final connection message sent by the client
    /// when the client wishes to kill the connection.
    /// </summary>
    [DebuggerDisplay("Client Connection Terminated Handler")]
    internal class ConnectionTerminateHandler : BaseOperationMessageHandler
    {
        /// <summary>
        /// Handles the message, executing the logic of this handler against it.
        /// </summary>
        /// <param name="clientProxy">The client proxy.</param>
        /// <param name="message">The message to be handled.</param>
        /// <returns>A newly set of messages (if any) to be sent back to the client.</returns>
        public override Task<IEnumerable<IGraphQLOperationMessage>> HandleMessage(
            ISubscriptionClientProxy clientProxy,
            IGraphQLOperationMessage message)
        {
            return Task.FromResult(Enumerable.Empty<IGraphQLOperationMessage>());
        }

        /// <summary>
        /// Gets the type of the message this handler can process.
        /// </summary>
        /// <value>The type of the message.</value>
        public override ApolloMessageType MessageType => ApolloMessageType.CONNECTION_TERMINATE;
    }
}