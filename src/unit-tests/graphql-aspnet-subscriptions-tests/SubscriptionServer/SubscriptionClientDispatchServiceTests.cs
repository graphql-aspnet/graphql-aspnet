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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.SubscriptionServer.BackgroundServices;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionClientDispatchServiceTests
    {
        [Test]
        public async Task ExecuteService_InvokesDispatchQueueWithoutError()
        {
            var dispatchQueue = Substitute.For<ISubscriptionEventDispatchQueue>();

            var service = new SubscriptionClientDispatchService(dispatchQueue);
#if NET8_0 || NET9_0
            var tokenSource = new CancellationTokenSource(15);
            await service.StartAsync(tokenSource.Token);
            service.Dispose();
#endif

            // with net10 there is a change to to how StartAsync works.
            // Under the hood ExecuteAsync (the protected method) used to be called directly
            // but with 10+ its called via Task.Run creating a race condition with this test
            // code where the service may dispose before the execute method actually fires.
            // Here we setup a longer delay and force a context switch to process that
            // scheduled task to ensure its execution in that runtime.
#if NET10_0_OR_GREATER
            var tokenSource = new CancellationTokenSource(1000);

            await service.StartAsync(tokenSource.Token);

            await Task.Yield();
            await Task.Delay(100);

            service.Dispose();
#endif

            await dispatchQueue.Received(1).BeginProcessingQueueAsync(Arg.Any<CancellationToken>());
        }
    }
}