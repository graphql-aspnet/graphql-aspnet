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
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Logging;
    using GraphQL.AspNet.Logging.SubscriptionEvents;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionClientDispatchQueueAlerterTests
    {
        [Test]
        public void NoThresholds_ExceptionIsThrown()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger.Object,
                    settings);
            });
        }

        [Test]
        public void NegativeTimeOnThreshold_ExceptionIsThrown()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Warning, 5, TimeSpan.FromSeconds(-5));

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger.Object,
                    settings);
            });
        }

        [Test]
        public void NegativeEventCount_ExceptionIsThrown()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Warning, -5, TimeSpan.FromSeconds(15));

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger.Object,
                    settings);
            });
        }

        [Test]
        public void ZeroEventCount_ExceptionIsThrown()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Warning, 0, TimeSpan.FromSeconds(15));

            Assert.Throws<ArgumentException>(() =>
            {
                var alerter = new SubscriptionClientDispatchQueueAlerter(
                    logger.Object,
                    settings);
            });
        }

        [Test]
        public void NoThresholdReached_NoEventRecorded()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger.Object,
                settings);

            alerter.CheckQueueCount(250);

            logger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Never());
        }

        [Test]
        public void ConfiguredThresholdIsLessThan1_1IsSetAsThreshold()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger.Object,
                settings,
                -1);

            Assert.AreEqual(1, alerter.CooldownToleranceMs);
        }

        [Test]
        public void ThresholdReachedExactly_EventIsRecorded()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger.Object,
                settings);

            alerter.CheckQueueCount(500);

            logger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());
        }

        [Test]
        public void ThresholdReachedCrossed_EventIsRecorded()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(LogLevel.Debug, 500, TimeSpan.FromMinutes(25));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger.Object,
                settings);

            alerter.CheckQueueCount(501);

            logger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());
        }

        [Test]
        public void ThresholdSkipped_CorrectThresholdIsRecorded()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

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
                logger.Object,
                settings);

            // skip the 500 level and go straight to 700 level
            // only 700 level should be recorded
            alerter.CheckQueueCount(701);

            logger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Never());

            logger.Verify(
               x => x.Log(
                   LogLevel.Warning,
                   SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                   It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
               Times.Once());
        }

        [Test]
        public async Task TwiceRaisedEvent_WhenCooldownNotReached_IsNotAlertedTwice()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

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
                logger.Object,
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
            logger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());
        }

        [Test]
        public async Task TwiceRaisedEvent_WhenCooldownReached_IsAlertedTwice()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

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
                logger.Object,
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
            logger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Exactly(2));
        }

        [Test]
        public void SuccessiveEventThresholds_AreAlertedAsExpected()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

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
                logger.Object,
                settings,
                10);

            alerter.CheckQueueCount(501);
            alerter.CheckQueueCount(10_001);
            alerter.CheckQueueCount(100_001);

            // ensure all events fired
            logger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());

            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());

            logger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());
        }

        [Test]
        public void AfterAllThresholdsAreAlerted_NoAdditionalAlertsSent()
        {
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var settings = new SubscriptionClientDispatchQueueAlertSettings();
            settings.AddThreshold(
                LogLevel.Critical,
                100_000,
                TimeSpan.FromMilliseconds(500));

            var alerter = new SubscriptionClientDispatchQueueAlerter(
                logger.Object,
                settings,
                10);

            alerter.CheckQueueCount(501); // no alert triggered
            alerter.CheckQueueCount(10_001); // alert should be triggered
            alerter.CheckQueueCount(10_001); // already alerted, skipped
            alerter.CheckQueueCount(100_001); // no more alert thresholds, skipped
            alerter.CheckQueueCount(1_000_001); // no more alert thresholds, skipped

            // ensure only single threshold event is fired
            logger.Verify(
                x => x.Log(
                    LogLevel.Critical,
                    SubscriptionLogEventIds.EventDispatchQueueThresholdReached,
                    It.IsAny<SubscriptionEventDispatchQueueAlertLogEntry>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<SubscriptionEventDispatchQueueAlertLogEntry, Exception, string>>()),
                Times.Once());
        }
    }
}