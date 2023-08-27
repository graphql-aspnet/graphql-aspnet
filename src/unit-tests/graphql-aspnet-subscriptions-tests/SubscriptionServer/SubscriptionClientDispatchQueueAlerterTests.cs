// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.SubscriptionServer
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionClientDispatchQueueAlerterTests
    {
        [Test]
        public void NoThresholds_ExceptionIsThrown()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger,
                    settings);
            });
        }

        [Test]
        public void NegativeTimeOnThreshold_ExceptionIsThrown()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Warning, 5, TimeSpan.FromSeconds(-5));

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger,
                    settings);
            });
        }

        [Test]
        public void NegativeEventCount_ExceptionIsThrown()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Warning, -5, TimeSpan.FromSeconds(15));

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger,
                    settings);
            });
        }

        [Test]
        public void ZeroEventCount_ExceptionIsThrown()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Warning, 0, TimeSpan.FromSeconds(15));

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger,
                    settings);
            });
        }

        [Test]
        public void NoThresholdReached_NoEventRecorded()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings);

            alerter.CheckQueueCount(250);

            logger.Received(0).Log(
                    Arg.Any<LogLevel>(),
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public void ConfiguredThresholdIsLessThan1_1IsSetAsThreshold()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings,
                -1);

            Assert.AreEqual(1, alerter.CooldownToleranceMs);
        }

        [Test]
        public void ThresholdReachedExactly_EventIsRecorded()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings);

            alerter.CheckQueueCount(500);

            logger.Received(1).Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public void ThresholdReachedCrossed_EventIsRecorded()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings);

            alerter.CheckQueueCount(501);

            logger.Received(1).Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public void ThresholdSkipped_CorrectThresholdIsRecorded()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(
                LogLevel.Debug,
                500,
                TimeSpan.FromMinutes(25));

            settings.AddThreshold(
                LogLevel.Warning,
                700,
                TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings);

            // skip the 500 level and go straight to 700 level
            // only 700 level should be recorded
            alerter.CheckQueueCount(701);

            logger.Received(0).Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());

            logger.Received(1).Log(
                   LogLevel.Warning,
                   SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                   Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                   Arg.Any<Exception>(),
                   Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public async Task TwiceRaisedEvent_WhenCooldownNotReached_IsNotAlertedTwice()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(
                LogLevel.Debug,
                500,
                TimeSpan.FromMilliseconds(500));

            settings.AddThreshold(
                LogLevel.Warning,
                700,
                TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings,
                10);

            // alert at level 500
            alerter.CheckQueueCount(501);

            // wait 50ms
            // threshold is within 10ms of a 500ms cooldown,
            // meaning the cooldown should NOT have been triggered yet
            await Task.Delay(50);

            // alert at level 500 a second time
            alerter.CheckQueueCount(501);

            // ensure both events fired
            logger.Received(1).Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public async Task TwiceRaisedEvent_WhenCooldownReached_IsAlertedTwice()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(
                LogLevel.Debug,
                500,
                TimeSpan.FromMilliseconds(10));

            settings.AddThreshold(
                LogLevel.Warning,
                700,
                TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings,
                10);

            // alert at level 500
            alerter.CheckQueueCount(501);

            // wait 50ms
            // threshold is within 10ms of a 10ms cooldown,
            // meaning the cooldown should have been triggered
            await Task.Delay(50);

            // alert at level 500 a second time
            alerter.CheckQueueCount(501);

            // ensure both events fired
            logger.Received(2).Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public void SuccessiveEventThresholds_AreAlertedAsExpected()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(
                LogLevel.Debug,
                500,
                TimeSpan.FromMilliseconds(500));

            settings.AddThreshold(
                LogLevel.Information,
                10_000,
                TimeSpan.FromMilliseconds(500));

            settings.AddThreshold(
                LogLevel.Critical,
                100_000,
                TimeSpan.FromMilliseconds(500));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings,
                10);

            alerter.CheckQueueCount(501);
            alerter.CheckQueueCount(10_001);
            alerter.CheckQueueCount(100_001);

            // ensure all events fired
            logger.Received(1).Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());

            logger.Received(1).Log(
                    LogLevel.Information,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());

            logger.Received(1).Log(
                    LogLevel.Critical,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }

        [Test]
        public void AfterAllThresholdsAreAlerted_NoAdditionalAlertsSent()
        {
            var logger = Substitute.For<ILogger>();
            logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(
                LogLevel.Critical,
                100_000,
                TimeSpan.FromMilliseconds(500));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger,
                settings,
                10);

            alerter.CheckQueueCount(501); // no alert triggered
            alerter.CheckQueueCount(10_001); // alert should be triggered
            alerter.CheckQueueCount(10_001); // already alerted, skipped
            alerter.CheckQueueCount(100_001); // no more alert thresholds, skipped
            alerter.CheckQueueCount(1_000_001); // no more alert thresholds, skipped

            // ensure only single threshold event is fired
            logger.Received(1).Log(
                    LogLevel.Critical,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    Arg.Any<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    Arg.Any<Exception>(),
                    Arg.Any<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>());
        }
    }
}