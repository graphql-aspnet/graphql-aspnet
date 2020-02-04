// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Messaging
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface to define an object that can process <see cref="IGraphQLOperationMessage"/>.
    /// </summary>
    internal interface IGraphQLOperationMessageHandler
    {
        /// <summary>
        /// Determines whether this instance can process the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if this instance can handle the specified message; otherwise, <c>false</c>.</returns>
        bool CanHandleMessage(IGraphQLOperationMessage message);

        /// <summary>
        /// Handles the message, executing the logic of this handler against it.
        /// </summary>
        /// <param name="clientProxy">The client proxy processing the message.</param>
        /// <param name="message">The message to be handled.</param>
        /// <returns>A newly set of messages (if any) to be sent back to the client.</returns>
        Task<IEnumerable<IGraphQLOperationMessage>> HandleMessage(
            ISubscriptionClientProxy clientProxy,
            IGraphQLOperationMessage message);
    }
}