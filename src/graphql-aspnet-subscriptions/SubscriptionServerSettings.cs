// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    /// <summary>
    /// "Server level" subscription and connection settings that govern all schemas registered
    /// to this server instance. These values rarely need to be changed, howver; any changes should be made prior to
    /// calling <c>.AddGraphQl()</c> during startup.
    /// </summary>
    public static class SubscriptionServerSettings
    {
        private static readonly object _lock = new object();
        private static int _maxConcurrentReceiverCount;
        private static int? _maxConnectedClientCount;

        /// <summary>
        /// Initializes static members of the <see cref="SubscriptionServerSettings"/> class.
        /// </summary>
        static SubscriptionServerSettings()
        {
            MaxConcurrentReceiverCount = 50;
        }

        /// <summary>
        /// Gets or sets the maximum number of receivers (clients) that the event router
        /// will communicate with at once. If the total number of receivers for any given subscription
        /// event exceeds this amount, the overflow will be queued until resources are made available.
        /// </summary>
        /// <remarks>
        /// (Default: 50, Minimum: 1).
        /// </remarks>
        /// <value>The maximum concurrent receiver count.</value>
        public static int MaxConcurrentReceiverCount
        {
            get
            {
                lock (_lock)
                    return _maxConcurrentReceiverCount;
            }

            set
            {
                lock (_lock)
                    _maxConcurrentReceiverCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of connected clients this server can handle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When set, this value enforces a maximum number of allowed clients across all schemas. If an additional
        /// client attempts to connect, it will be immediately closed with an appropriate error.
        /// </para>
        /// <para>
        /// In practical terms, this value represents the number of allowed websocket connections.
        /// </para>
        /// <para>
        /// (Default: ~no limit~).
        /// </para>
        /// </remarks>
        /// <value>The maximum connected client count.</value>
        public static int? MaxConnectedClientCount
        {
            get
            {
                lock (_lock)
                    return _maxConnectedClientCount;
            }

            set
            {
                lock (_lock)
                    _maxConnectedClientCount = value;
            }
        }
    }
}