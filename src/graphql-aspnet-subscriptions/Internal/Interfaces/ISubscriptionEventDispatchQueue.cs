// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;

    /// <summary>
    /// A dispatch queue that will forward events to receivers on a
    /// throttled, asyncronous scheduled.
    /// </summary>
    public interface ISubscriptionEventDispatchQueue : IDisposable
    {
        /// <summary>
        /// Enqueues the event to be processed as resources become available.
        /// </summary>
        /// <param name="clientId">The id of the client that should receive the event.</param>
        /// <param name="eventData">The event data.</param>
        /// <param name="closeAfter">if set to <c>true</c> The queue will be closed
        /// and no further events will be accepted after this event is scheduled.</param>
        /// <returns><c>true</c> if the event data was successfully queued,
        /// <c>false</c> otherwise.</returns>
        bool EnqueueEvent(SubscriptionClientId clientId, SubscriptionEvent eventData, bool closeAfter = false);

        /// <summary>
        /// Instructs this queue to stop processing events,forever. The queue should not be started
        /// or restarted once this method is called.
        /// </summary>
        void StopQueue();

        /// <summary>
        /// Starts a task that should only complete when all events, for all time, are done
        /// being processed. If the queue is already actively being processed
        /// this method should quietly exits without failure.
        /// </summary>
        /// <remarks>
        /// This method should throw an exception if called after <see cref="StopQueue"/> has been called.
        /// </remarks>
        /// <param name="cancelToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Task.</returns>
        Task BeginProcessingQueueAsync(CancellationToken cancelToken = default);

        /// <summary>
        /// Gets a value indicating whether the queue is being processed.
        /// </summary>
        /// <value><c>true</c> if this instance is processing; otherwise, <c>false</c>.</value>
        bool IsProcessing { get; }

        /// <summary>
        /// Gets the maximum number of events being processed at a time.
        /// </summary>
        /// <value>The maximum concurrent events.</value>
        int MaxConcurrentEvents { get; }
    }
}