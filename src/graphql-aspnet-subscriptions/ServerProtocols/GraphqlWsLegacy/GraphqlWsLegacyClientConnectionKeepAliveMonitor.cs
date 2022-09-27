﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy
{
    using System;
    using System.Threading;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Connections.Clients;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.ServerProtocols.GraphqlWsLegacy.Messages.ServerMessages;

    /// <summary>
    /// Sends a periodic keep-alive message that conforms to the expectations of the GraphqlWsLegacy graphql-over-websockets
    /// spec.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema proxies are bound to.</typeparam>
    internal class GraphqlWsLegacyClientConnectionKeepAliveMonitor<TSchema> : IDisposable
        where TSchema : class, ISchema
    {
        private readonly GraphqlWsLegacyClientProxy<TSchema> _connection;
        private readonly TimeSpan _interval;
        private Timer _timer;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphqlWsLegacyClientConnectionKeepAliveMonitor{TSchema}" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="interval">The interval.</param>
        public GraphqlWsLegacyClientConnectionKeepAliveMonitor(GraphqlWsLegacyClientProxy<TSchema> connection, TimeSpan interval)
        {
            _connection = Validation.ThrowIfNullOrReturn(connection, nameof(connection));
            _interval = interval;
        }

        private void TimeForKeepAlive(object state)
        {
            if (_connection == null || _connection.State != ClientConnectionState.Open)
            {
                this.Stop();
            }
            else
            {
                _connection.SendMessage(new GraphqlWsLegacyKeepAliveOperationMessage());
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