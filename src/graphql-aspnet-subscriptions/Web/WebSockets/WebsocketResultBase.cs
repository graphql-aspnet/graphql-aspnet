// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Connections.WebSockets
{
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A base class abstracting data related to a recieved message through a websocket.
    /// </summary>
    public abstract class WebsocketResultBase : IClientConnectionReceiveResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the data received is the end of a signle message.
        /// </summary>
        /// <value><c>true</c> if [end of message]; otherwise, <c>false</c>.</value>
        public bool EndOfMessage { get; protected set; }

        /// <inheritdoc />
        public ConnectionCloseStatus? CloseStatus { get; protected set; }

        /// <inheritdoc />
        public string CloseStatusDescription { get; protected set; }

        /// <inheritdoc />
        public int Count { get; protected set; }

        /// <inheritdoc />
        public ClientMessageType MessageType { get; protected set; }
    }
}