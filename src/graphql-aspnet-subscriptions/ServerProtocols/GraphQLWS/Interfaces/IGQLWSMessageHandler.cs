// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphQLWS.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.ServerProtocols.GraphQLWS.Messages.Common;

    /// <summary>
    /// An interface to define an object that can process <see cref="GQLWSMessage"/>.
    /// </summary>
    internal interface IGQLWSMessageHandler
    {
        /// <summary>
        /// Determines whether this instance can process the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if this instance can handle the specified message; otherwise, <c>false</c>.</returns>
        bool CanHandleMessage(GQLWSMessage message);

        /// <summary>
        /// Handles the message, executing the logic of this handler against it.
        /// </summary>
        /// <param name="clientProxy">The client proxy processing the message.</param>
        /// <param name="message">The message to be handled.</param>
        /// <returns>A newly set of messages (if any) to be sent back to the client.</returns>
        Task<IEnumerable<GQLWSMessage>> HandleMessage(ISubscriptionClientProxy clientProxy, GQLWSMessage message);
    }
}