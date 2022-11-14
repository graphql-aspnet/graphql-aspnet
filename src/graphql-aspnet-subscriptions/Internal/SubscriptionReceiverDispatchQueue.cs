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
    using GraphQL.AspNet.Interfaces.Subscriptions;

    using QueuedEvent = System.ValueTuple<AspNet.Interfaces.Subscriptions.ISubscriptionEventReceiver, Execution.Subscriptions.SubscriptionEvent>;

    /// <summary>
    /// The default dispatch queue to throttle subscription events being sent to expectant receivers.
    /// </summary>
    internal sealed class SubscriptionReceiverDispatchQueue : ISubscriptionReceiverDispatchQueue
    {
        private readonly Channel<QueuedEvent> _queue;
        private readonly CancellationTokenSource _stopRequested;
        private readonly SemaphoreSlim _throttle;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionReceiverDispatchQueue" /> class.
        /// </summary>
        /// <param name="maxConcurrentEvents">The maximum number of concurrent events
        /// that can be executing.</param>
        /// <param name="maxOverallEvents">The maximum number of events this queue can process. Once reached, no
        /// additional events can be processed. (null == no limit).</param>
        public SubscriptionReceiverDispatchQueue(int maxConcurrentEvents, int? maxOverallEvents = null)
        {
            _queue = maxOverallEvents.HasValue
                ? Channel.CreateBounded<QueuedEvent>(maxOverallEvents.Value)
                : Channel.CreateUnbounded<QueuedEvent>();

            if (maxConcurrentEvents < 1)
                maxConcurrentEvents = 1;

            this.MaxConcurrentEvents = maxConcurrentEvents;
            _stopRequested = new CancellationTokenSource();
            _throttle = new SemaphoreSlim(this.MaxConcurrentEvents);
        }

        /// <inheritdoc />
        public bool EnqueueEvent(ISubscriptionEventReceiver receiver, SubscriptionEvent eventData, bool closeAfter = false)
        {
            Validation.ThrowIfNull(receiver, nameof(receiver));
            Validation.ThrowIfNull(eventData, nameof(eventData));
            var evt = new QueuedEvent(receiver, eventData);

            var queued = _queue.Writer.TryWrite(evt);
            if (closeAfter || !queued)
                _queue.Writer.Complete();

            return queued;
        }

        /// <inheritdoc />
        public void StopQueue()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SubscriptionReceiverDispatchQueue));

            _queue.Writer.TryComplete();
            _stopRequested.Cancel();
        }

        /// <inheritdoc />
        public async Task BeginProcessingQueue(CancellationToken cancelToken = default)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SubscriptionReceiverDispatchQueue));

            if (_stopRequested.IsCancellationRequested)
                throw new InvalidOperationException("Dispatch queue has already been stopped and cannot be restarted.");

            try
            {
                var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _stopRequested.Token);
                this.IsProcessing = true;

                // wait for data
                while (await _queue.Reader.WaitToReadAsync(combinedToken.Token))
                {
                    if (_isDisposed)
                        return;

                    while (_queue.Reader.TryRead(out var evt))
                    {
                        if (_isDisposed)
                            return;

                        // wait for an execution slot
                        await _throttle.WaitAsync(combinedToken.Token);
                        _ = this.DispatchEvent(evt, combinedToken.Token);
                    }
                }
            }
            finally
            {
                this.IsProcessing = false;
            }
        }

        private async ValueTask DispatchEvent(QueuedEvent evt, CancellationToken cancelToken = default)
        {
            try
            {
                if (_isDisposed)
                    return;

                var receiver = evt.Item1;
                var eventData = evt.Item2;
                await receiver.ReceiveEvent(eventData, cancelToken);
            }
            finally
            {
                _throttle.Release();
            }
        }

        /// <inheritdoc />
        public bool IsProcessing { get; private set; }

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