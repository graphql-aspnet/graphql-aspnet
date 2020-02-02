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
    using GraphQL.AspNet.Interfaces.Messaging;
    using GraphQL.AspNet.Messaging.ServerMessages;

    /// <summary>
    /// A handler for processing the initial connection message sent by the client
    /// after the websocket has been established.
    /// </summary>
    [DebuggerDisplay("Client Connection Initalized Handler")]
    internal class ConnectionInitHandler : BaseOperationMessageHandler
    {
        /// <summary>
        /// Handles the message, executing the logic of this handler against it.
        /// </summary>
        /// <param name="message">The message to be handled.</param>
        /// <returns>A newly set of messages (if any) to be sent back to the client.</returns>
        public override IEnumerable<IGraphQLOperationMessage> HandleMessage(IGraphQLOperationMessage message)
        {
            yield return new ServerAckOperationMessage();
            yield return new KeepAliveOperationMessage();
        }

        /// <summary>
        /// Gets the type of the message this handler can process.
        /// </summary>
        /// <value>The type of the message.</value>
        public override GraphQLOperationMessageType MessageType => GraphQLOperationMessageType.CONNECTION_INIT;
    }
}