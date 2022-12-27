// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Subscriptions;

    /// <summary>
    /// A globally shared, intermediate queue of <see cref="SubscriptionEvent"/> items waiting to be published. When
    /// controllers publish events they are initially staged to this queue where an additional, continuous,
    /// service dequeues them and publishes them using the server's configured <see cref="ISubscriptionEventPublisher"/>.
    /// </summary>
    /// <remarks>
    /// This class is a wrapper on the built in Channel{T} queue provided by dotnet.
    /// </remarks>
    public sealed class SubscriptionEventPublishingQueue
    {
        private Channel<SubscriptionEvent> _publishChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventPublishingQueue"/> class.
        /// </summary>
        public SubscriptionEventPublishingQueue()
        {
            _publishChannel = Channel.CreateUnbounded<SubscriptionEvent>();
        }

        /// <summary>
        /// Enqueues the event to be published. Publishing occurs periodically when allowed
        /// by the runtime.
        /// </summary>
        /// <param name="evt">The event to enqueue.</param>
        /// <returns><c>true</c> if the event was successfully queued, <c>false</c> otherwise.</returns>
        public bool Enqueue(SubscriptionEvent evt)
        {
            Validation.ThrowIfNull(evt, nameof(evt));
            return _publishChannel.Writer.TryWrite(evt);
        }

        /// <summary>
        /// Wait until data is ready to be read from the queue.
        /// </summary>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task&lt;ValueTask&gt; representing the asynchronous operation.</returns>
        public async ValueTask<bool> WaitToDequeueAsync(CancellationToken cancelToken = default)
        {
            return await _publishChannel.Reader.WaitToReadAsync(cancelToken);
        }

        /// <summary>
        /// Attempts to read an event from the queue.
        /// </summary>
        /// <param name="dequeuedEvent">The event that was found and dequeued.</param>
        /// <returns><c>true</c> if an event was read, <c>false</c> otherwise.</returns>
        public bool TryDequeue(out SubscriptionEvent dequeuedEvent)
        {
            return _publishChannel.Reader.TryRead(out dequeuedEvent);
        }
    }
}