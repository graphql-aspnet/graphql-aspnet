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

            var tokenSource = new CancellationTokenSource(15);
            await service.StartAsync(tokenSource.Token);
            service.Dispose();

            await dispatchQueue.Received(1).BeginProcessingQueueAsync(Arg.Any<CancellationToken>());
        }
    }
}