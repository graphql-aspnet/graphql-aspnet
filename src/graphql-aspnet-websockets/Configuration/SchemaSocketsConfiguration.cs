// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.Configuration;

    /// <summary>
    /// A set of configuration options available for setting up your websocket support for
    /// subscriptions for your server. These settings apply universally to all schemas supporting subscriptions.
    /// </summary>
    [DebuggerDisplay("Schema Declaration Configuration")]
    public class SchemaSocketsConfiguration : ISchemaSocketsConfiguration
    {
        /// <summary>
        /// Gets or sets the interval, in milliseconds, that the server will send
        /// ping data to a connected client to keep proxies open (Default: 2 minutes).
        /// </summary>
        /// <value>The keep alive interval in milliseconds.</value>
        public int KeepAliveIntervalInMilliseconds { get; set; } = 2 * 60 * 1000;

        /// <summary>
        /// Gets or sets the size of the data receive buffer (Default: 4kb).
        /// </summary>
        /// <value>The size of the receive buffer.</value>
        public int ReceiveBufferSize { get; set; } = 4 * 1024;
    }
}