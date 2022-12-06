// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using NUnit.Framework;

    [TestFixture]
    public class QueryCancellationMonitorTests
    {
        [Test]
        public async Task WhenTimerExpires_MonitorIsSetToTimeout()
        {
            var contextTokenSource = new CancellationTokenSource(100);

            var monitor = new QueryCancellationMonitor(contextTokenSource.Token, TimeSpan.FromMilliseconds(50));
            monitor.Start();

            await monitor.MonitorTask;

            Assert.IsTrue(monitor.IsTimedOut);
            monitor.Dispose();
        }

        [Test]
        public async Task WhenExternalTokenExpires_MonitorIsSetToCancelled()
        {
            var contextTokenSource = new CancellationTokenSource(50);

            var monitor = new QueryCancellationMonitor(contextTokenSource.Token, TimeSpan.FromMilliseconds(100));
            monitor.Start();

            await monitor.MonitorTask;

            Assert.IsTrue(monitor.IsCancelled);
            monitor.Dispose();
        }

        [Test]
        public async Task WhenMonitorIsManuallyCompleted_MonitorIsSetToCompleted()
        {
            var contextTokenSource = new CancellationTokenSource(100);
            var monitor = new QueryCancellationMonitor(contextTokenSource.Token, TimeSpan.FromMilliseconds(100));
            monitor.Start();

            monitor.Complete();

            await monitor.MonitorTask;

            Assert.IsTrue(monitor.IsCompleted);
            monitor.Dispose();
        }

        [Test]
        public async Task WhenNoTimeoutPeriodSet_ExternalTokenStillCancelsMonitor()
        {
            var contextTokenSource = new CancellationTokenSource(50);
            var monitor = new QueryCancellationMonitor(contextTokenSource.Token);
            monitor.Start();

            await monitor.MonitorTask;

            Assert.IsTrue(monitor.IsCancelled);
            monitor.Dispose();
        }

        [Test]
        public async Task WhenNoExternalTokenProvided_TimeoutStillOccurs()
        {
            var monitor = new QueryCancellationMonitor(timeoutPeriod: TimeSpan.FromMilliseconds(35));
            monitor.Start();

            await monitor.MonitorTask;

            Assert.IsTrue(monitor.IsTimedOut);
            monitor.Dispose();
        }
    }
}