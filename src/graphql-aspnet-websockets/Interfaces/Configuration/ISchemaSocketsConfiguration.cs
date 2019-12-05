// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface defining the various configuration options available for setting up your websocket support for
    /// subscriptions for your server. These settings apply universally to all schemas supporting subscriptions.
    /// </summary>
    public interface ISchemaSocketsConfiguration
    {
        /// <summary>
        /// Gets or sets the interval, in milliseconds, that the server will send
        /// ping data to a connected client to keep proxies open (Default: 2 minutes).
        /// </summary>
        /// <value>The keep alive interval in milliseconds.</value>
        int KeepAliveIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the size of the data receive buffer (Default: 4kb).
        /// </summary>
        /// <value>The size of the receive buffer.</value>
        int ReceiveBufferSize { get; set; }
    }
}