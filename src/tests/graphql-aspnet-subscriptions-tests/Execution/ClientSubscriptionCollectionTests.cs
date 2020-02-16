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
    using System.Linq;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.Structural;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ClientSubscriptionCollectionTests
    {
        private ISubscription<GraphSchema> MakeSubscription(string id = "abc123", string routePath = "path1/path2")
        {
            var subscription = new Mock<ISubscription<GraphSchema>>();

            var field = new Mock<ISubscriptionGraphField>();
            var path = new GraphFieldPath(AspNet.Execution.GraphCollection.Subscription, routePath);
            field.Setup(x => x.Route).Returns(path);
            subscription.Setup(x => x.Route).Returns(path);
            subscription.Setup(x => x.Field).Returns(field.Object);
            subscription.Setup(x => x.IsValid).Returns(true);
            subscription.Setup(x => x.ClientProvidedId).Returns(id);

            var subClient = new Mock<ISubscriptionClientProxy>();
            subscription.Setup(x => x.Client).Returns(subClient.Object);

            return subscription.Object;
        }

        [Test]
        public void ClientAdded_IsReturnedWhenSearched()
        {
            var subscription = this.MakeSubscription();
            var collection = new ClientSubscriptionCollection<GraphSchema>();

            collection.Add(subscription);

            var foundSubs = collection.RetrieveSubscriptions(subscription.Client);
            Assert.AreEqual(1, foundSubs.Count());
            Assert.AreEqual(1, collection.RetrieveSubscriptions(subscription.Route).Count());

            Assert.AreEqual(subscription, foundSubs.Single());
        }

        [Test]
        public void ClientRemoved_NoLongerReturnedBySearch()
        {
            var subscription = this.MakeSubscription("abc124");
            var collection = new ClientSubscriptionCollection<GraphSchema>();

            collection.Add(subscription);

            // ensure it was added
            var foundSubs = collection.RetrieveSubscriptions(subscription.Client);
            Assert.AreEqual(1, foundSubs.Count());

            var foundSub = foundSubs.Single();

            // try and take it out
            var removedSub = collection.TryRemoveSubscription(subscription.Client, "abc124");

            // ensure the returned item is the one that was originally added
            Assert.IsNotNull(removedSub);
            Assert.AreEqual(foundSub, removedSub);

            // ensure nothing exists that can be found
            Assert.AreEqual(0, foundSubs.Count());
            Assert.AreEqual(0, collection.RetrieveSubscriptions(subscription.Route).Count());
        }

        [Test]
        public void EventRegistered_OnlyTriggeredOnFirstAdditionOfEvent()
        {
            var subscription = this.MakeSubscription("abc124");
            var subscription2 = this.MakeSubscription("abc125");
            var collection = new ClientSubscriptionCollection<GraphSchema>();

            var totalInvocations = 0;
            collection.SubscriptionFieldRegistered += (sender, args) =>
            {
                totalInvocations += 1;
            };

            collection.Add(subscription);
            collection.Add(subscription2);

            Assert.AreEqual(1, totalInvocations);
            Assert.AreEqual(2, collection.RetrieveSubscriptions(subscription.Field.Route).Count());
        }

        [Test]
        public void EventAbandoned_OnlyTriggeredOnLastRemovalOfEvent()
        {
            var subscription = this.MakeSubscription("abc124");
            var subscription2 = this.MakeSubscription("abc125");
            var collection = new ClientSubscriptionCollection<GraphSchema>();

            var totalInvocations = 0;
            collection.SubscriptionFieldAbandoned += (sender, args) =>
            {
                totalInvocations += 1;
            };

            collection.Add(subscription);
            collection.Add(subscription2);

            // take out one, ensure event not triggered
            var subRemoved = collection.TryRemoveSubscription(subscription2.Client, subscription2.ClientProvidedId);
            Assert.AreEqual(subRemoved, subscription2);
            Assert.AreEqual(0, totalInvocations);

            // take out last one ensure event triggered
            subRemoved = collection.TryRemoveSubscription(subscription.Client, subscription.ClientProvidedId);
            Assert.AreEqual(subRemoved, subscription);
            Assert.AreEqual(1, totalInvocations);
        }
    }
}