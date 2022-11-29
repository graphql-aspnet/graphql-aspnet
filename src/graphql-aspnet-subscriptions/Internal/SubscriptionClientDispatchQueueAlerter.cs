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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// An internal object that monitors configured alert thresholds for the dispatch queue
    /// and raises events accordingly.
    /// </summary>
    internal class SubscriptionClientDispatchQueueAlerter
    {
        private readonly object _locker = new object();
        private readonly ILogger _logger;
        private readonly int _cooldownToleranceMs;
        private readonly SubscriptionEventAlertThreshold[] _allThresholds;
        private readonly DateTimeOffset[] _cooldownExpirations;
        private readonly HashSet<Task> _cooldownTasks;

        private int _nextThresholdIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientDispatchQueueAlerter" /> class.
        /// </summary>
        /// <param name="logger">The logger to record log events to.</param>
        /// <param name="alertSettings">The alert settings to use when deteremining
        /// if an event should be recorded.</param>
        /// <param name="cooldownToleranceInMs">A
        /// tolerance, in milliseconds, that if the expected cooldown time vs the actual time
        /// is within this tolerance the cooldown will be considered completed (default 5 seconds).</param>
        public SubscriptionClientDispatchQueueAlerter(
            ILogger logger,
            ISubscriptionClientDispatchQueueAlertSettings alertSettings,
            int cooldownToleranceInMs = 5000)
        {
            Validation.ThrowIfNull(alertSettings, nameof(alertSettings));
            _logger = Validation.ThrowIfNullOrReturn(logger, nameof(logger));
            _cooldownToleranceMs = cooldownToleranceInMs;

            if (_cooldownToleranceMs <= 0)
                _cooldownToleranceMs = 1;

            if (alertSettings.AlertThresholds.Count == 0)
                throw new ArgumentException("At least one alert threshold must be provided.", nameof(alertSettings));

            foreach (var threshold in alertSettings.AlertThresholds)
            {
                if (threshold.CooldownPeriod.TotalSeconds < 0)
                    throw new ArgumentException("All alert thresholds must define a positive amount of time as the cooldown period.", nameof(alertSettings));

                if (threshold.SubscriptionEventCountThreshold <= 0)
                    throw new ArgumentException("All alert thresholds must define an event count greater than 0 to check for.", nameof(alertSettings));
            }

            _allThresholds = alertSettings.AlertThresholds
                .OrderBy(x => x.SubscriptionEventCountThreshold)
                .ToArray();

            _nextThresholdIndex = 0;

            _cooldownExpirations = new DateTimeOffset[_allThresholds.Length];
            _cooldownTasks = new HashSet<Task>();

            for (var i = 0; i < _cooldownExpirations.Length; i++)
                _cooldownExpirations[i] = DateTimeOffset.MinValue;
        }

        /// <summary>
        /// Checks the supplied queue count against the configured thresholds
        /// and raises an event if and when necessary.
        /// </summary>
        /// <param name="count">The current count on the dispatch queue.</param>
        public void CheckQueueCount(int count)
        {
            if (!this.ShouldAlert(count))
                return;

            lock (_locker)
            {
                if (!this.ShouldAlert(count))
                    return;

                this.DispatchAlert(count);
            }
        }

        private bool ShouldAlert(int count)
        {
            // ensure that we're not beyond the maximum alerting threshold
            // when we are then there can be no more alerting
            if (_nextThresholdIndex >= _allThresholds.Length)
                return false;

            // has the next threshold been reached?
            var nextThresholdValue = _allThresholds[_nextThresholdIndex].SubscriptionEventCountThreshold;
            return count >= nextThresholdValue;
        }

        private void DispatchAlert(int count)
        {
            var now = DateTimeOffset.UtcNow;

            // its possible that when we alert we need to skip a level
            // and that we really need to alert on something higher
            // (e.g. the next level is 500 but the count is suddenly 751; we want to alert
            // at the 750 threshold, not 500, if it exists).
            var actualNextIndex = _nextThresholdIndex;
            for (var i = _nextThresholdIndex + 1; i < _allThresholds.Length; i++)
            {
                if (_allThresholds[i].SubscriptionEventCountThreshold > count)
                    break;

                actualNextIndex = i;
            }

            // dispatch the event
            var thresholdBeingAlerted = _allThresholds[actualNextIndex];
            _logger.SubscriptionEventDispatchQueueThresholdReached(
                count,
                thresholdBeingAlerted);

            // calculate the cooldown timeframe
            _cooldownExpirations[actualNextIndex] = now.Add(thresholdBeingAlerted.CooldownPeriod);

            // advance to the threshold
            _nextThresholdIndex = actualNextIndex + 1;

            // begin the task for the cooldown time after which
            // we reevalate what the next threshold should be allowing it to be lowered
            // if necessary
            var delayTask = Task.Delay(thresholdBeingAlerted.CooldownPeriod);
            _cooldownTasks.Add(delayTask);
            delayTask.ContinueWith((task) =>
            {
                _cooldownTasks.Remove(task);
                this.EvaluateCooldownPeriods();
            });
        }

        private void EvaluateCooldownPeriods()
        {
            var now = DateTimeOffset.UtcNow;

            lock (_locker)
            {
                // start at the top (most critical) and evaluate down the threshold levels
                // to deteremine what is the next threshold level that should be reached
                //
                // if a lower level has expired (e.g. the event at a queue of 10_000 has expired, but
                // not the event of a queue at 100_000) don't expire the lower level yet
                //
                // we don't want a situation encountered where an alert for 5_000 events
                // is triggered when the queue is really still at 100_000.
                for (var i = _allThresholds.Length - 1; i >= 0; i--)
                {
                    if (_cooldownExpirations[i] == DateTimeOffset.MinValue)
                        continue;

                    var difference = _cooldownExpirations[i].Subtract(now).TotalMilliseconds;
                    if (difference > _cooldownToleranceMs)
                    {
                        // if the threshold  for cool down has not been reached then
                        // this level is not finished cooling down.
                        // The next level to alert on is the one before this one
                        _nextThresholdIndex = i + 1;
                        return;
                    }

                    // if we're within 5 seconds of expiring just consider it expired
                    // this should be enough to account for delays or potential race conditions
                    // in the delay tasks completing
                    // the expirations are ment to be in minute intervals so as to prevent spamming
                    _cooldownExpirations[i] = DateTimeOffset.MinValue;
                }

                _nextThresholdIndex = 0;
            }
        }
    }
}