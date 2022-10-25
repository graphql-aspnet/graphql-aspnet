﻿// *************************************************************
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
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Internal;
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

            Assert.Throws<ArgumentNullException>(() =>
            {
                router.RaisePublishedEvent(null);
            });
        }

        [Test]
        public void RemovingANullReceiverDoesNothing()
        {
            var router = new DefaultSubscriptionEventRouter();
            router.RemoveReceiver(null);
        }

        [Test]
        public void SubscribedReceiver_ReceivesRaisedEvent()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            var dispatcher = new Mock<ISubscriptionReceiverDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher.Object);

            var evt = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent5",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            router.AddReceiver(receiver.Object, evt.ToSubscriptionEventName());
            router.RaisePublishedEvent(evt);

            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt, false), Times.Once, "Event1 never received");
        }

        [Test]
        public void SubscribedReceiveer_TwoEvents_BothEventsAreDispatchedToTheReceiver()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            var dispatcher = new Mock<ISubscriptionReceiverDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher.Object);

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
            router.AddReceiver(receiver.Object, evt.ToSubscriptionEventName());
            router.AddReceiver(receiver.Object, evt2.ToSubscriptionEventName());
            router.RaisePublishedEvent(evt);
            router.RaisePublishedEvent(evt2);

            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt, false), Times.Once, "Event1 never received");
            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt2, false), Times.Once, "Event2 never received");
        }

        [Test]
        public void UnsubscribedReceiver_DoesNotReceivesRaisedEvent()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            var dispatcher = new Mock<ISubscriptionReceiverDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher.Object);

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
            router.AddReceiver(receiver.Object, evt.ToSubscriptionEventName());
            router.AddReceiver(receiver.Object, evt2.ToSubscriptionEventName());
            router.RemoveReceiver(receiver.Object, evt.ToSubscriptionEventName());
            router.RaisePublishedEvent(evt);
            router.RaisePublishedEvent(evt2);

            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt, false), Times.Never, "Event1 was received");
            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt2, false), Times.Once, "Event2 never received");
        }

        [Test]
        public void UnsubscribedAllReceiver_DoesNotReceivesRaisedEvent()
        {
            var receiver = new Mock<ISubscriptionEventReceiver>();
            var dispatcher = new Mock<ISubscriptionReceiverDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher.Object);

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
            router.AddReceiver(receiver.Object, evt.ToSubscriptionEventName());
            router.AddReceiver(receiver.Object, evt2.ToSubscriptionEventName());
            router.RemoveReceiver(receiver.Object);
            router.RaisePublishedEvent(evt);
            router.RaisePublishedEvent(evt2);

            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt, false), Times.Never, "Event1 was received");
            dispatcher.Verify(x => x.EnqueueEvent(receiver.Object, evt2, false), Times.Never, "Event2 was received");
        }
    }
}