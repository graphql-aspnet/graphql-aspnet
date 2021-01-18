// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Apollo
{
    using System;
    using System.Threading;
    using GraphQL.AspNet.Apollo.Messages.ServerMessages;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// Sends a periodic keep-alive message that conforms to the expectations of the apollo graphql-over-websockets
    /// spec.
    /// </summary>
    internal class ApolloClientConnectionKeepAliveMonitor
    {
        private readonly ISubscriptionClientProxy _connection;
        private readonly TimeSpan _interval;
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApolloClientConnectionKeepAliveMonitor"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interval">The interval.</param>
        public ApolloClientConnectionKeepAliveMonitor(ISubscriptionClientProxy connection, TimeSpan interval)
        {
            _connection = Validation.ThrowIfNullOrReturn(connection, nameof(connection));
            _interval = interval;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ApolloClientConnectionKeepAliveMonitor"/> class.
        /// </summary>
        ~ApolloClientConnectionKeepAliveMonitor()
        {
            _timer?.Dispose();
        }

        private void TimeForKeepAlive(object state)
        {
            if (_connection == null || _connection.State != ClientConnectionState.Open)
            {
                this.Stop();
            }
            else
            {
                _connection.SendMessage(new ApolloKeepAliveOperationMessage());
            }
        }

        /// <summary>
        /// Starts this instance sending the first keep alive after the initial interval has passed.
        /// </summary>
        public void Start()
        {
            _timer?.Dispose();
            _timer = new Timer(this.TimeForKeepAlive, null, _interval, _interval);
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