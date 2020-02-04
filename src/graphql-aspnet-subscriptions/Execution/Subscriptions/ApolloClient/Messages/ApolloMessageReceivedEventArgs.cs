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
    public class ApolloMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloMessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ApolloMessageReceivedEventArgs(IApolloMessage message)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets the message that was recieved by the client.
        /// </summary>
        /// <value>The message.</value>
        public IApolloMessage Message { get; }
    }
}