﻿// *************************************************************
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
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class InProcessPublisherTests
    {
        [Test]
        public async Task PublishEvent_ForwardsEventToRouter()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            router.Setup(x => x.RaisePublishedEvent(It.IsAny<SubscriptionEvent>()));

            var publisher = new InProcessSubscriptionPublisher(router.Object);

            var eventData = new SubscriptionEvent();
            await publisher.PublishEventAsync(eventData);

            router.Verify(x => x.RaisePublishedEvent(It.IsAny<SubscriptionEvent>()), Times.Once(), "failed to raise the event");
        }
    }
}