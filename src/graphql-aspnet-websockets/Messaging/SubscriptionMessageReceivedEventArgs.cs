// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Messaging
{
    using System;
    using GraphQL.AspNet.Interfaces.Messaging;

    /// <summary>
    /// A set of arguments carried when a graphql subscription message is recieved from a connected client.
    /// </summary>
    public class SubscriptionMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionMessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SubscriptionMessageReceivedEventArgs(IGraphQLOperationMessage message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the message that was recieved by the client.
        /// </summary>
        /// <value>The message.</value>
        public IGraphQLOperationMessage Message { get; }
    }
}