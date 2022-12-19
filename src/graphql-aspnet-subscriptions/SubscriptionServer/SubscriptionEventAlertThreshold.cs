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
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A single alert threshold level for queued subscription events
    /// that will result in a log message if reached.
    /// </summary>
    [DebuggerDisplay("Alert Level: {SubscriptionEventCountThreshold} events")]
    public class SubscriptionEventAlertThreshold
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionEventAlertThreshold" /> class.
        /// </summary>
        /// <param name="logLevel">The log level to record the event at.</param>
        /// <param name="eventCount">The number of queued events
        /// at which point this threshold will be reached.</param>
        /// <param name="cooldownPeriod">The cooldown period after raising a log message
        /// before another log message of this threshold will be raised.</param>
        /// <param name="customMessage">A custom message
        /// that will be appended to the log entry generated when this threshold is reached.
        /// This can be useful for quickly searching for specific data within a log analytics platform.</param>
        public SubscriptionEventAlertThreshold(
            LogLevel logLevel,
            int eventCount,
            TimeSpan cooldownPeriod,
            string customMessage = null)
        {
            this.LogLevel = logLevel;
            this.SubscriptionEventCountThreshold = eventCount;
            this.CooldownPeriod = cooldownPeriod;
            this.CustomMessage = customMessage;
        }

        /// <summary>
        /// Gets the log level at which the event will be raised.
        /// </summary>
        /// <value>The log level.</value>
        public LogLevel LogLevel { get; }

        /// <summary>
        /// Gets the number of subscription events that must be queued up
        /// in order for this threshold to be met.
        /// </summary>
        /// <value>The total number of queued events that are necessary for this
        /// threshold to be met.</value>
        public int SubscriptionEventCountThreshold { get; }

        /// <summary>
        /// Gets the amount of time after this threshold is reached that must pass
        /// before a log event of this threshold can be recorded again.
        /// </summary>
        /// <value>The required cooldown period between log events of this
        /// threshold.</value>
        public TimeSpan CooldownPeriod { get; }

        /// <summary>
        /// Gets a custom message that will be added to the recorded log event.
        /// </summary>
        /// <value>The custom message appended to the log event when this threshold is reached.</value>
        public string CustomMessage { get; }
    }
}