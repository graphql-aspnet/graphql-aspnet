// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The default dispatch queue to throttle subscription events being sent to expectant receivers.
    /// This object accepts events targeted at specific clients at an uncontrolled rate
    /// then throttles their transmission to said client according to configured rules.
    /// </summary>
    internal sealed class SubscriptionClientDispatchQueue : ISubscriptionEventDispatchQueue
    {
        private readonly IGlobalSubscriptionClientProxyCollection _clientCollection;
        private readonly Channel<ValueTuple<SubscriptionClientId, SubscriptionEvent>> _queue;
        private readonly SubscriptionClientDispatchQueueAlerter _alerter;
        private readonly CancellationTokenSource _stopRequested;
        private readonly SemaphoreSlim _throttle;

        private CancellationTokenSource _combinedTokenSource;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientDispatchQueue" /> class.
        /// </summary>
        /// <param name="clientCollection">The global collection containing a reference to
        /// all actively connected clients.</param>
        /// <param name="alertSettings">A configured collection of settings
        /// used to instruct the dispatch queue on when it should raise alerts about the
        /// number of queued events.</param>
        /// <param name="loggerFactory">The logger factory from which
        /// a logger instance will be made to record events from this dispatch queue instance.</param>
        /// <param name="maxConcurrentEvents">The maximum number of concurrent events
        /// that can be executing simultaniously. When null, the globally configured default is used.</param>
        /// <param name="maxOverallEvents">The maximum number of events this queue can process. Once reached, no
        /// additional events can be processed. (null == no limit). In general, this value should be
        /// passed as <c>null</c> in all production scenarios.</param>
        public SubscriptionClientDispatchQueue(
            IGlobalSubscriptionClientProxyCollection clientCollection,
            ISubscriptionClientDispatchQueueAlertSettings alertSettings = null,
            ILoggerFactory loggerFactory = null,
            int? maxConcurrentEvents = null,
            int? maxOverallEvents = null)
        {
            _clientCollection = Validation.ThrowIfNullOrReturn(clientCollection, nameof(clientCollection));

            _queue = maxOverallEvents.HasValue
                ? Channel.CreateBounded<ValueTuple<SubscriptionClientId, SubscriptionEvent>>(maxOverallEvents.Value)
                : Channel.CreateUnbounded<ValueTuple<SubscriptionClientId, SubscriptionEvent>>();

            if (loggerFactory != null)
            {
                var logger = loggerFactory.CreateLogger(Constants.Logging.LOG_CATEGORY);
                alertSettings = alertSettings ?? SubscriptionConstants.Alerts.DefaultDispatchQueueAlertSettings;
                _alerter = new SubscriptionClientDispatchQueueAlerter(logger, alertSettings);
            }

            if (!maxConcurrentEvents.HasValue)
                maxConcurrentEvents = GraphQLSubscriptionServerSettings.MaxConcurrentSubscriptionReceiverCount;

            if (maxConcurrentEvents < 1)
                maxConcurrentEvents = 1;

            this.MaxConcurrentEvents = maxConcurrentEvents.Value;
            _stopRequested = new CancellationTokenSource();
            _throttle = new SemaphoreSlim(this.MaxConcurrentEvents);
        }

        /// <inheritdoc />
        public bool EnqueueEvent(SubscriptionClientId clientId, SubscriptionEvent eventData, bool closeAfter = false)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SubscriptionClientDispatchQueue));

            Validation.ThrowIfNull(eventData, nameof(eventData));
            var evt = (clientId, eventData);

            var queued = _queue.Writer.TryWrite(evt);
            if (closeAfter || !queued)
                _queue.Writer.Complete();

            if (_alerter != null && _queue.Reader.CanCount)
            {
                _alerter.CheckQueueCount(_queue.Reader.Count);
            }

            return queued;
        }

        /// <inheritdoc />
        public void StopQueue()
        {
            // if its disposed...its already stopped
            if (_isDisposed)
                return;

            _queue.Writer.TryComplete();
            _stopRequested.Cancel();
        }

        /// <inheritdoc />
        public async Task BeginProcessingQueueAsync(CancellationToken cancelToken = default)
        {
            if (this.IsProcessing)
                return;

            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SubscriptionClientDispatchQueue));

            if (_stopRequested.IsCancellationRequested)
                throw new InvalidOperationException("Dispatch queue has already been stopped and cannot be restarted.");

            try
            {
                this.IsProcessing = true;
                if (_combinedTokenSource != null)
                    _combinedTokenSource.Dispose();

                _combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _stopRequested.Token);

                // wait for data
                while (await _queue.Reader.WaitToReadAsync(_combinedTokenSource.Token))
                {
                    if (_isDisposed)
                        return;

                    if (cancelToken.IsCancellationRequested)
                        return;

                    while (_queue.Reader.TryRead(out var evt))
                    {
                        if (_isDisposed)
                            return;

                        // wait for an execution slot
                        await _throttle.WaitAsync(_combinedTokenSource.Token);

                        _ = this.DispatchEventAsync(
                                evt.Item1,
                                evt.Item2,
                                _combinedTokenSource.Token);
                    }
                }
            }
            finally
            {
                this.IsProcessing = false;
            }
        }

        private async ValueTask DispatchEventAsync(SubscriptionClientId clientId, SubscriptionEvent eventData, CancellationToken cancelToken = default)
        {
            try
            {
                if (_isDisposed)
                    return;

                if (_clientCollection.TryGetClient(clientId, out var client))
                {
                    await client.ReceiveEventAsync(eventData, cancelToken);
                }
            }
            finally
            {
                _throttle.Release();
            }
        }

        /// <inheritdoc />
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// Gets the number of events currently in the queue.
        /// </summary>
        /// <value>The count of queued events.</value>
        public int Count
        {
            get
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(SubscriptionClientDispatchQueue));

                return _queue.Reader.Count;
            }
        }

        /// <inheritdoc />
        public int MaxConcurrentEvents { get; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    this.StopQueue();
                    _throttle.Dispose();
                    _combinedTokenSource?.Dispose();
                    _stopRequested.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}