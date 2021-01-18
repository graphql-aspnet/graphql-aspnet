// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Execution
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class InProcessPublisherTests
    {
        [Test]
        public async Task PublishEvent_ForwardsEventToRouter()
        {
            var router = new Mock<ISubscriptionEventRouter>();
            router.Setup(x => x.RaiseEvent(It.IsAny<SubscriptionEvent>())).Returns(Task.CompletedTask);

            var publisher = new InProcessSubscriptionPublisher(router.Object);

            var eventData = new SubscriptionEvent();
            await publisher.PublishEvent(eventData);

            router.Verify(x => x.RaiseEvent(It.IsAny<SubscriptionEvent>()), Times.Once(), "failed to raise the event");
        }
    }
}