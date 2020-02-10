// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Subscriptions
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions.Apollo;

    /// <summary>
    /// An interface representing an established connection to a client that can process
    /// subscription data from the server.
    /// </summary>
    public interface ISubscriptionClientProxy
    {
        /// <summary>
        /// Performs acknowledges the setup of the subscription through the websocket and brokers messages
        /// between the client and the graphql runtime for its lifetime. When this method completes the socket is
        /// closed.
        /// </summary>
        /// <returns>Task.</returns>
        Task StartConnection();

        /// <summary>
        /// Gets the service provider instance assigned to this client for resolving object requests.
        /// </summary>
        /// <value>The service provider.</value>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> representing the user of the client.
        /// </summary>
        /// <value>The user.</value>
        ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets the state of the underlying connection.
        /// </summary>
        /// <value>The state.</value>
        ClientConnectionState State { get; }

        /// <summary>
        /// Serializes, encodes and sends the given message down to the client.
        /// </summary>
        /// <param name="message">The message to be serialized and sent to the client.</param>
        /// <returns>Task.</returns>
        Task SendMessage(object message);
    }
}