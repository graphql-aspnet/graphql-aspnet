// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Internal.Interfaces;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A default implementation of <see cref="ISubscriptionClientDispatchQueueAlertSettings"/>.
    /// </summary>
    public class SubscriptionClientDispatchQueueAlertSettings : ISubscriptionClientDispatchQueueAlertSettings
    {
        private List<SubscriptionEventAlertThreshold> _allSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientDispatchQueueAlertSettings"/> class.
        /// </summary>
        public SubscriptionClientDispatchQueueAlertSettings()
        {
            _allSettings = new List<SubscriptionEventAlertThreshold>();
        }

        /// <summary>
        /// Adds a new threshold level to this collection of settings.
        /// </summary>
        /// <param name="logLevel">The log level the event will be recorded at.</param>
        /// <param name="eventCount">The event count that must be reached for a log
        /// event is recorded.</param>
        /// <param name="cooldown">The cooldown period that must elapse before this threshold
        /// is reached again.</param>
        /// <param name="customMessage">A custom message that will be added to the logged event, if any.</param>
        public void AddThreshold(LogLevel logLevel, int eventCount, TimeSpan cooldown, string customMessage = null)
        {
            _allSettings.Add(new SubscriptionEventAlertThreshold(logLevel, eventCount, cooldown));
        }

        /// <inheritdoc />
        public IReadOnlyList<SubscriptionEventAlertThreshold> AlertThresholds => _allSettings;
    }
}