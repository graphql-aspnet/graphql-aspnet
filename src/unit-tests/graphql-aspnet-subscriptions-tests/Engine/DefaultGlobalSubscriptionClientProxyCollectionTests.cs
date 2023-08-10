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
    using Castle.Components.DictionaryAdapter.Xml;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.SubscriptionServer;
    using Microsoft.VisualStudio.TestPlatform.Utilities;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultGlobalSubscriptionClientProxyCollectionTests
    {
        [Test]
        public void AddClient_WhenMaxNotReached_ClientIsAdded()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(5);

            var client = Substitute.For<ISubscriptionClientProxy>();
            client.Id.Returns(SubscriptionClientId.NewClientId());

            var result = collection.TryAddClient(client);

            Assert.IsTrue(result);
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void AddClient_WhenMaxReached_ClientIsRejected()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var client = Substitute.For<ISubscriptionClientProxy>();
            client.Id.Returns(SubscriptionClientId.NewClientId());

            var client2 = Substitute.For<ISubscriptionClientProxy>();
            client2.Id.Returns(SubscriptionClientId.NewClientId());

            collection.TryAddClient(client);
            var result = collection.TryAddClient(client2);

            Assert.IsFalse(result);
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void TryRemoveClient_ForExistingClient_ClientIsRemoved()
        {
            var collection = new DefaultGlobalSubscriptionClientProxyCollection(1);

            var client = Substitute.For<ISubscriptionClientProxy>();
            client.Id.Returns(SubscriptionClientId.NewClientId());

            collection.TryAddClient(client);
            Assert.AreEqual(1, collection.Count);

            var result = collection.TryRemoveClient(client.Id, out var obj);

            Assert.IsTrue(result);
            Assert.AreEqual(client, obj);
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

            var client = Substitute.For<ISubscriptionClientProxy>();
            client.Id.Returns(SubscriptionClientId.NewClientId());

            collection.TryAddClient(client);

            var result = collection.TryGetClient(client.Id, out var obj);

            Assert.IsTrue(result);
            Assert.AreEqual(client, obj);
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