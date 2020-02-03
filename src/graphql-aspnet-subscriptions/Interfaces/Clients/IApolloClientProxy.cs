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
    using System;
    using System.Net.WebSockets;
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface representing an established connection to an apollog compatiable client
    /// that is connected and can recieve data.
    /// </summary>
    public interface IApolloClientProxy
    {
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
        /// Gets the state of the underlying websocket connection.
        /// </summary>
        /// <value>The state.</value>
        WebSocketState State { get; }

        /// <summary>
        /// Serializes, encodes and sends the given message down to the client.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>Task.</returns>
        Task SendMessage(IGraphQLOperationMessage message);
    }
}