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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Messaging;

    /// <summary>
    /// A handler for processing client operation stop requests.
    /// </summary>
    [DebuggerDisplay("Client Operation Stopped Handler")]
    internal class ApolloSubscriptionStoppedHandler : ApolloMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloSubscriptionStoppedHandler"/> class.
        /// </summary>
        public ApolloSubscriptionStoppedHandler()
        {
        }

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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the type of the message this handler can process.
        /// </summary>
        /// <value>The type of the message.</value>
        public override ApolloMessageType MessageType => ApolloMessageType.STOP;
    }
}