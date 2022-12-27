// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.SubscriptionEvents
{
    /// <summary>
    /// A configurable graph log entry that is raised conditionally based on
    /// a certian number of queued events in the subscription event dispatch queue.
    /// </summary>
    public class SubscriptionEventDispatchQueueAlertLogEntry : GraphLogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventDispatchQueueAlertLogEntry"/> class.
        /// </summary>
        /// <param name="thresholdLevelReached">The threshold level reached.</param>
        /// <param name="queueCount">The queue count.</param>
        /// <param name="customMessage">The custom message.</param>
        public SubscriptionEventDispatchQueueAlertLogEntry(
            int thresholdLevelReached,
            int queueCount,
            string customMessage = null)
            : base(SubscriptionLogEventIds.EventDispatchQueueThresholdReached)
        {
            this.ThresholdLevelReached = thresholdLevelReached;
            this.EventQueueCount = queueCount;
            this.CustomMessage = customMessage?.Trim();
        }

        /// <summary>
        /// Gets the configured event queue threshold level that was reached which
        /// triggered this log entry.
        /// </summary>
        /// <value>The triggered threshold level, in number of events.</value>
        public int ThresholdLevelReached
        {
            get => this.GetProperty<int>(SubscriptionLogPropertyNames.SUBSCRIPTION_DISPATCH_QUEUE_THRESHOLD_LEVEL);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_DISPATCH_QUEUE_THRESHOLD_LEVEL, value);
        }

        /// <summary>
        /// Gets the number of events in the queue when this message was raised.
        /// </summary>
        /// <value>The number of events in the queue when this message was raised.</value>
        public int EventQueueCount
        {
            get => this.GetProperty<int>(SubscriptionLogPropertyNames.SUBSCRIPTION_DISPATCH_QUEUE_COUNT);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_DISPATCH_QUEUE_COUNT, value);
        }

        /// <summary>
        /// Gets a custom configured message supplied via this threshold level.
        /// </summary>
        /// <value>The custom configured message, if any.</value>
        public string CustomMessage
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_DISPATCH_QUEUE_THRESHOLD_MESSAGE);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_DISPATCH_QUEUE_THRESHOLD_MESSAGE, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Dispatch Queue Threshold Reached | Threshold: {this.ThresholdLevelReached}, Actual: {this.EventQueueCount}";
        }
    }
}