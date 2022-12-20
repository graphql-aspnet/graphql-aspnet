// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Engine
{
    using Castle.Components.DictionaryAdapter.Xml;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.VisualStudio.TestPlatform.Utilities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultGlobalSubscriptionClientProxyCollectionTests
    {
        [Test]
        public void AddClient_WhenMaxNotReached_ClientIsAdded()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(5);

            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns(SubscriptionClientId.NewClientId());

            var result = collection.TryAddClient(client.Object);

            Assert.IsTrue(result);
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void AddClient_WhenMaxReached_ClientIsRejected()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns(SubscriptionClientId.NewClientId());

            var client2 = new Mock<ISubscriptionClientProxy>();
            client2.Setup(x => x.Id).Returns(SubscriptionClientId.NewClientId());

            collection.TryAddClient(client.Object);
            var result = collection.TryAddClient(client2.Object);

            Assert.IsFalse(result);
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void TryRemoveClient_ForExistingClient_ClientIsRemoved()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns(SubscriptionClientId.NewClientId());

            collection.TryAddClient(client.Object);
            Assert.AreEqual(1, collection.Count);

            var result = collection.TryRemoveClient(client.Object.Id, out var obj);

            Assert.IsTrue(result);
            Assert.AreEqual(client.Object, obj);
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void TryRemoveClient_ForNotExistingClient_ClientIsNotRemoved()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var id = SubscriptionClientId.NewClientId();

            var result = collection.TryRemoveClient(id, out var obj);

            Assert.IsFalse(result);
            Assert.IsNull(obj);
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void TryRemoveClient_ForNullId_FalseIsReturned()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var result = collection.TryRemoveClient(null, out var obj);

            Assert.IsFalse(result);
            Assert.IsNull(obj);
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void TryGetClient_ForExistingClient_ClientIsReturned()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var client = new Mock<ISubscriptionClientProxy>();
            client.Setup(x => x.Id).Returns(SubscriptionClientId.NewClientId());

            collection.TryAddClient(client.Object);

            var result = collection.TryGetClient(client.Object.Id, out var obj);

            Assert.IsTrue(result);
            Assert.AreEqual(client.Object, obj);
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void TryGetClient_ForNotExistingClient_ClientIsNotReturned()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var id = SubscriptionClientId.NewClientId();

            var result = collection.TryGetClient(id, out var obj);

            Assert.IsFalse(result);
            Assert.IsNull(obj);
            Assert.AreEqual(0, collection.Count);
        }
    }
}