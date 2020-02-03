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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Messaging;

    /// <summary>
    /// A handler for processing client operation stop requests.
    /// </summary>
    internal class UnknownMessageHandler : IGraphQLOperationMessageHandler
    {
        /// <summary>
        /// Determines whether this instance can process the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if this instance can handle the specified message; otherwise, <c>false</c>.</returns>
        public bool CanHandleMessage(IGraphQLOperationMessage message)
        {
            return true;
        }

        /// <summary>
        /// Handles the message, executing the logic of this handler against it.
        /// </summary>
        /// <param name="clientProxy">The client proxy.</param>
        /// <param name="message">The message to be handled.</param>
        /// <returns>A newly set of messages (if any) to be sent back to the client.</returns>
        public Task<IEnumerable<IGraphQLOperationMessage>> HandleMessage(
            ISubscriptionClientProxy clientProxy,
            IGraphQLOperationMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}