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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.SubscriptionServer;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class InProcessPublisherTests
    {
        [Test]
        public async Task PublishEvent_ForwardsEventToRouter()
        {
            var router = Substitute.For<ISubscriptionEventRouter>();

            var publisher = new InProcessSubscriptionPublisher(router);

            var eventData = new SubscriptionEvent();
            await publisher.PublishEventAsync(eventData);

            router.Received(1).RaisePublishedEvent(Arg.Any<SubscriptionEvent>());
        }
    }
}