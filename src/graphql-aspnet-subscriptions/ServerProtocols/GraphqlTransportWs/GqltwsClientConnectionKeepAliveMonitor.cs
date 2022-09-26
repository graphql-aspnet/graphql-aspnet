// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlTransportWs
{
    using System;
    using System.Threading;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.ServerProtocols.GraphqlTransportWs.Messages.BidirectionalMessages;

    /// <summary>
    /// Sends a periodic keep-alive message down the wire to the connected client
    /// that conforms to the expectations of the graphql-ws spec.
    /// </summary>
    internal class GqltwsClientConnectionKeepAliveMonitor : IDisposable
    {
        private readonly ISubscriptionClientProxy _client;
        private readonly TimeSpan _interval;
        private Timer _timer;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GqltwsClientConnectionKeepAliveMonitor"/> class.
        /// </summary>
        /// <param name="client">The connection to keep alive.</param>
        /// <param name="interval">The interval on which to inititate the PING/PONG cycle.</param>
        public GqltwsClientConnectionKeepAliveMonitor(ISubscriptionClientProxy client, TimeSpan interval)
        {
            _client = Validation.ThrowIfNullOrReturn(client, nameof(client));
            _interval = interval;
        }

        private void TimeForKeepAlive(object state)
        {
            if (_client.State != ClientConnectionState.Open)
            {
                this.Stop();
            }
            else
            {
                _client.SendMessage(new GqltwsPingMessage());
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}