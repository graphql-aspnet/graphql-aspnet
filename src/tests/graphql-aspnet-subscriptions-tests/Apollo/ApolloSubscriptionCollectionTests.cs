// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Apollo
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Apollo;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ApolloSubscriptionCollectionTests
    {
        [Test]
        public void AddNewSub_ReflectsInCollectionCount()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            var field = new GraphFieldPath("[subscription]/field1");
            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            collection.Add(fakeSub.Object);
            Assert.AreEqual(1, collection.CountByRoute(field));
            Assert.AreEqual(1, collection.Count);
        }

        [Test]
        public void AddNewSub_OfSameRouteButDiffererntId_ReflectsInCollectionCount()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            var field = new GraphFieldPath("[subscription]/field1");
            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            var fakeSub2 = new Mock<ISubscription<GraphSchema>>();
            fakeSub2.Setup(x => x.Id).Returns("abc1234");
            fakeSub2.Setup(x => x.Route).Returns(field);

            collection.Add(fakeSub.Object);
            collection.Add(fakeSub2.Object);
            Assert.AreEqual(2, collection.CountByRoute(field));
            Assert.AreEqual(2, collection.Count);
        }

        [Test]
        public void AddNewSub_OfDifferentRoute_ReflectsInCollectionCount()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            var field = new GraphFieldPath("[subscription]/field1");
            var field2 = new GraphFieldPath("[subscription]/field2");
            var field3 = new GraphFieldPath("[subscription]/field3");

            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            var fakeSub2 = new Mock<ISubscription<GraphSchema>>();
            fakeSub2.Setup(x => x.Id).Returns("abc1234");
            fakeSub2.Setup(x => x.Route).Returns(field2);

            collection.Add(fakeSub.Object);
            collection.Add(fakeSub2.Object);
            Assert.AreEqual(1, collection.CountByRoute(field));
            Assert.AreEqual(1, collection.CountByRoute(field2));
            Assert.AreEqual(2, collection.Count);

            var foundSubs = collection.RetreiveByRoute(field);
            Assert.AreEqual(fakeSub.Object, foundSubs.Single());

            foundSubs = collection.RetreiveByRoute(field2);
            Assert.AreEqual(fakeSub2.Object, foundSubs.Single());

            foundSubs = collection.RetreiveByRoute(field3);
            CollectionAssert.IsEmpty(foundSubs);

            var counted = 0;
            foreach (var sub in collection)
            {
                Assert.IsTrue(sub == fakeSub.Object || sub == fakeSub2.Object);
                counted++;
            }

            Assert.AreEqual(2, counted);
        }

        [Test]
        public void RemoveExistingSub_ReflectsInCollectionCount()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            var field = new GraphFieldPath("[subscription]/field1");
            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            collection.Add(fakeSub.Object);
            Assert.AreEqual(1, collection.Count);

            collection.Remove(fakeSub.Object.Id, out var subOut);

            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(subOut, fakeSub.Object);
        }

        [Test]
        public void AddExistingSubId_ThrowsException()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();
            var field = new GraphFieldPath("[subscription]/field1");

            var fakeSub = new Mock<ISubscription<GraphSchema>>();
            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            var fakeSub2 = new Mock<ISubscription<GraphSchema>>();
            fakeSub2.Setup(x => x.Id).Returns("abc123");
            fakeSub2.Setup(x => x.Route).Returns(field);

            collection.Add(fakeSub.Object);

            Assert.Throws<ArgumentException>(() => collection.Add(fakeSub.Object));
        }

        [Test]
        public void RemoveNonExistingSub_NoSubReturned()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            collection.Remove("bobSub", out var subOut);

            Assert.AreEqual(0, collection.Count);
            Assert.IsNull(subOut);
        }

        [Test]
        public void AddNewSub_NotReturnedOnInvalidRoute()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            var field = new GraphFieldPath("[subscription]/field1");
            var field2 = new GraphFieldPath("[wrong]/field2");
            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            collection.Add(fakeSub.Object);
            Assert.AreEqual(0, collection.CountByRoute(field2));
        }

        [Test]
        public void AddNewSub_NotReturnedOnNullRoute()
        {
            var collection = new ApolloSubscriptionCollection<GraphSchema>();

            var fakeSub = new Mock<ISubscription<GraphSchema>>();

            var field = new GraphFieldPath("[subscription]/field1");
            fakeSub.Setup(x => x.Id).Returns("abc123");
            fakeSub.Setup(x => x.Route).Returns(field);

            collection.Add(fakeSub.Object);
            Assert.AreEqual(0, collection.CountByRoute(null));
        }
    }
}