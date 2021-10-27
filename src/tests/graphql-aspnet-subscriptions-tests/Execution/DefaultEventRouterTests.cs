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
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultEventRouterTests
    {
        [Test]
        public void NullEventResultsInException()
        {
            var router = new DefaultSubscriptionEventRouter();

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await router.RaiseEvent(null);
            });
        }

        [Test]
        public void RemovingANullReceiverDoesNothing()
        {
            var router = new DefaultSubscriptionEventRouter();
            router.RemoveReceiver(null);
        }

        [Test]
        public async Task SubscribedReceiver_ReceivesRaisedEvent()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            receiver.Setup(x => x.ReceiveEvent(It.IsAny<SubscriptionEvent>())).Returns(Task.CompletedTask);

            var router = new DefaultSubscriptionEventRouter();

            var evt = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent5",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            router.AddReceiver(evt.ToSubscriptionEventName(), receiver.Object);
            await router.RaiseEvent(evt);

            receiver.Verify(x => x.ReceiveEvent(evt));
        }

        [Test]
        public async Task SubscribedReceiveer_TwoEvents_DoesReceiveBothEventsOnce()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            receiver.Setup(x => x.ReceiveEvent(It.IsAny<SubscriptionEvent>())).Returns(Task.CompletedTask);

            var router = new DefaultSubscriptionEventRouter();

            var evt = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent5",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            var evt2 = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent6",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            // add two events but remove the one being raised
            // to ensure its not processed
            router.AddReceiver(evt.ToSubscriptionEventName(), receiver.Object);
            router.AddReceiver(evt2.ToSubscriptionEventName(), receiver.Object);
            await router.RaiseEvent(evt);
            await router.RaiseEvent(evt2);

            receiver.Verify(x => x.ReceiveEvent(evt), Times.Once);
            receiver.Verify(x => x.ReceiveEvent(evt2), Times.Once);
        }

        [Test]
        public async Task UnsubscribedReceiver_DoesNotReceivesRaisedEvent()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            receiver.Setup(x => x.ReceiveEvent(It.IsAny<SubscriptionEvent>())).Returns(Task.CompletedTask);

            var router = new DefaultSubscriptionEventRouter();

            var evt = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent5",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            var evt2 = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent6",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            // add two events but remove the one being raised
            // to ensure its not processed
            router.AddReceiver(evt.ToSubscriptionEventName(), receiver.Object);
            router.AddReceiver(evt2.ToSubscriptionEventName(), receiver.Object);
            router.RemoveReceiver(evt.ToSubscriptionEventName(), receiver.Object);
            await router.RaiseEvent(evt);
            await router.RaiseEvent(evt2);

            receiver.Verify(x => x.ReceiveEvent(evt), Times.Never);
            receiver.Verify(x => x.ReceiveEvent(evt2), Times.Once);
        }

        [Test]
        public async Task UnsubscribedAllReceiver_DoesNotReceivesRaisedEvent()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            receiver.Setup(x => x.ReceiveEvent(It.IsAny<SubscriptionEvent>())).Returns(Task.CompletedTask);

            var router = new DefaultSubscriptionEventRouter();

            var evt = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent5",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            var evt2 = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent6",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            // add two events and ensure both are removed when not directly named
            router.AddReceiver(evt.ToSubscriptionEventName(), receiver.Object);
            router.AddReceiver(evt2.ToSubscriptionEventName(), receiver.Object);
            router.RemoveReceiver(receiver.Object);
            await router.RaiseEvent(evt);
            await router.RaiseEvent(evt2);

            receiver.Verify(x => x.ReceiveEvent(evt), Times.Never);
            receiver.Verify(x => x.ReceiveEvent(evt2), Times.Never);
        }
    }
}