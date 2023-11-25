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
    using System;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.SubscriptionServer;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionCollectionTests
    {
        [Test]
        public void AddNewSub_ReflectsInCollectionCount()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            var field = new ItemPath("[subscription]/field1");
            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            collection.Add(fakeSub);
            Assert.AreEqual(1, collection.CountByPath(field));
            Assert.AreEqual(1, collection.Count);

            Assert.AreEqual(1, collection.Keys.Count());
            Assert.AreEqual(1, collection.Values.Count());
            Assert.AreEqual(fakeSub, collection["abc123"]);
            Assert.IsTrue(collection.ContainsKey("abc123"));
        }

        [Test]
        public void AddNewSub_OfSameRouteButDiffererntId_ReflectsInCollectionCount()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            var field = new ItemPath("[subscription]/field1");
            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            var fakeSub2 = Substitute.For<ISubscription<GraphSchema>>();
            fakeSub2.Id.Returns("abc1234");
            fakeSub2.ItemPath.Returns(field);

            collection.Add(fakeSub);
            collection.Add(fakeSub2);
            Assert.AreEqual(2, collection.CountByPath(field));
            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void AddNewSub_OfDifferentRoute_ReflectsInCollectionCount()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            var field = new ItemPath("[subscription]/field1");
            var field2 = new ItemPath("[subscription]/field2");
            var field3 = new ItemPath("[subscription]/field3");

            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            var fakeSub2 = Substitute.For<ISubscription<GraphSchema>>();
            fakeSub2.Id.Returns("abc1234");
            fakeSub2.ItemPath.Returns(field2);

            collection.Add(fakeSub);
            collection.Add(fakeSub2);
            Assert.AreEqual(1, collection.CountByPath(field));
            Assert.AreEqual(1, collection.CountByPath(field2));
            Assert.AreEqual(2, collection.Count);

            var foundSubs = collection.RetreiveByItemPath(field);
            Assert.AreEqual(fakeSub, foundSubs.Single());

            foundSubs = collection.RetreiveByItemPath(field2);
            Assert.AreEqual(fakeSub2, foundSubs.Single());

            foundSubs = collection.RetreiveByItemPath(field3);
            CollectionAssert.IsEmpty(foundSubs);

            var counted = 0;
            foreach (var sub in collection)
            {
                Assert.IsTrue(sub == fakeSub || sub == fakeSub2);
                counted++;
            }

            Assert.AreEqual(2, counted);
        }

        [Test]
        public void RemoveExistingSub_ReflectsInCollectionCount()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            var field = new ItemPath("[subscription]/field1");
            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            collection.Add(fakeSub);
            Assert.AreEqual(1, collection.Count);

            collection.Remove(fakeSub.Id, out var subOut);

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(subOut, fakeSub);
        }

        [Test]
        public void AddExistingSubId_ThrowsException()
        {
            var collection = new SubscriptionCollection<GraphSchema>();
            var field = new ItemPath("[subscription]/field1");

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();
            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            var fakeSub2 = Substitute.For<ISubscription<GraphSchema>>();
            fakeSub2.Id.Returns("abc123");
            fakeSub2.ItemPath.Returns(field);

            collection.Add(fakeSub);

            Assert.Throws<ArgumentException>(() => collection.Add(fakeSub));
        }

        [Test]
        public void RemoveNonExistingSub_NoSubReturned()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            collection.Remove("bobSub", out var subOut);

            Assert.AreEqual(0, collection.Count);
            Assert.IsNull(subOut);
        }

        [Test]
        public void AddNewSub_NotReturnedOnInvalidRoute()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            var field = new ItemPath("[subscription]/field1");
            var field2 = new ItemPath("[wrong]/field2");
            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            collection.Add(fakeSub);
            Assert.AreEqual(0, collection.CountByPath(field2));
        }

        [Test]
        public void AddNewSub_NotReturnedOnNullRoute()
        {
            var collection = new SubscriptionCollection<GraphSchema>();

            var fakeSub = Substitute.For<ISubscription<GraphSchema>>();

            var field = new ItemPath("[subscription]/field1");
            fakeSub.Id.Returns("abc123");
            fakeSub.ItemPath.Returns(field);

            collection.Add(fakeSub);
            Assert.AreEqual(0, collection.CountByPath(null));
        }
    }
}