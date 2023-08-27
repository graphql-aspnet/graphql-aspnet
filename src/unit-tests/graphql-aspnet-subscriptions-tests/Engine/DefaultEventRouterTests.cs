// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultEventRouterTests
    {
        [Test]
        public void NullEventResultsInException()
        {
            var dispatcher = Substitute.For<ISubscriptionEventDispatchQueue>();
            var router = new DefaultSubscriptionEventRouter(dispatcher);

            Assert.Throws<ArgumentNullException>(() =>
            {
                router.RaisePublishedEvent(null);
            });

            router.Dispose();
        }

        [Test]
        public void RemovingANullReceiverDoesNothing()
        {
            var dispatcher = Substitute.For<ISubscriptionEventDispatchQueue>();
            var router = new DefaultSubscriptionEventRouter(dispatcher);

            router.RemoveClient(null);

            router.Dispose();
        }

        [Test]
        public void SubscribedReceiver_ReceivesRaisedEvent()
        {
            var receiver = Substitute.For<ISubscriptionClientProxy>();
            receiver.Id.Returns(SubscriptionClientId.NewClientId());
            var dispatcher = Substitute.For<ISubscriptionEventDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher);

            var evt = new SubscriptionEvent()
            {
                SchemaTypeName = typeof(GraphSchema).FullName,
                EventName = "bobEvent5",
                DataTypeName = typeof(TwoPropertyObject).FullName,
                Data = new TwoPropertyObject(),
            };

            router.AddClient(receiver, evt.ToSubscriptionEventName());
            router.RaisePublishedEvent(evt);

            dispatcher.Received(1).EnqueueEvent(receiver.Id, evt, false);

            router.Dispose();
        }

        [Test]
        public void SubscribedReceiveer_TwoEvents_BothEventsAreDispatchedToTheReceiver()
        {
            var receiver = Substitute.For<ISubscriptionClientProxy>();
            receiver.Id.Returns(SubscriptionClientId.NewClientId());

            var dispatcher = Substitute.For<ISubscriptionEventDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher);

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
            router.AddClient(receiver, evt.ToSubscriptionEventName());
            router.AddClient(receiver, evt2.ToSubscriptionEventName());
            router.RaisePublishedEvent(evt);
            router.RaisePublishedEvent(evt2);

            dispatcher.Received(1).EnqueueEvent(receiver.Id, evt, false);
            dispatcher.Received(1).EnqueueEvent(receiver.Id, evt2, false);

            router.Dispose();
        }

        [Test]
        public void UnsubscribedReceiver_DoesNotReceivesRaisedEvent()
        {
            var receiver = Substitute.For<ISubscriptionClientProxy>();
            receiver.Id.Returns(SubscriptionClientId.NewClientId());

            var dispatcher = Substitute.For<ISubscriptionEventDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher);

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
            router.AddClient(receiver, evt.ToSubscriptionEventName());
            router.AddClient(receiver, evt2.ToSubscriptionEventName());
            router.RemoveClient(receiver, evt.ToSubscriptionEventName());
            router.RaisePublishedEvent(evt);
            router.RaisePublishedEvent(evt2);

            dispatcher.Received(0).EnqueueEvent(receiver.Id, evt, false);
            dispatcher.Received(1).EnqueueEvent(receiver.Id, evt2, false);

            router.Dispose();
        }

        [Test]
        public void UnsubscribedAllReceiver_DoesNotReceivesRaisedEvent()
        {
            var receiver = Substitute.For<ISubscriptionClientProxy>();
            receiver.Id.Returns(SubscriptionClientId.NewClientId());

            var dispatcher = Substitute.For<ISubscriptionEventDispatchQueue>();

            var router = new DefaultSubscriptionEventRouter(dispatchQueue: dispatcher);

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
            router.AddClient(receiver, evt.ToSubscriptionEventName());
            router.AddClient(receiver, evt2.ToSubscriptionEventName());
            router.RemoveClient(receiver);
            router.RaisePublishedEvent(evt);
            router.RaisePublishedEvent(evt2);

            dispatcher.Received(0).EnqueueEvent(receiver.Id, evt, false);
            dispatcher.Received(0).EnqueueEvent(receiver.Id, evt2, false);

            router.Dispose();
        }
    }
}