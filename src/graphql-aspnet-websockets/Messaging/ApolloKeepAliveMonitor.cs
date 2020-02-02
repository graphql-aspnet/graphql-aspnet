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
    using System.Net.WebSockets;
    using System.Threading;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// Replaces the default keep alive message sent by ASP.NET Core websockets with a message
    /// specifically for the Apollo GraphQL protocol.
    /// </summary>
    internal class ApolloKeepAliveMonitor
    {
        private readonly WebSocket _socket;
        private readonly TimeSpan _interval;
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloKeepAliveMonitor"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="interval">The interval.</param>
        public ApolloKeepAliveMonitor(WebSocket socket, TimeSpan interval)
        {
            _socket = Validation.ThrowIfNullOrReturn(socket, nameof(socket));
            _interval = interval;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ApolloKeepAliveMonitor"/> class.
        /// </summary>
        ~ApolloKeepAliveMonitor()
        {
            _timer?.Dispose();
        }

        private void TimeForKeepAlive(object state)
        {
            if (_socket == null || _socket.State != WebSocketState.Open)
            {
                this.Stop();
            }
            else
            {
                lock (_socket)
                {
                }
            }
        }

        /// <summary>
        /// Starts this instance sending the first keep alive after the initial interval has passed.
        /// </summary>
        public void Start()
        {
            _timer?.Dispose();
            _timer = new Timer(TimeForKeepAlive, null, _interval, _interval);
        }

        /// <summary>
        /// Stops this instance, no more keep alive messages will be sent.
        /// </summary>
        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();
            _timer = null;
        }
    }
}