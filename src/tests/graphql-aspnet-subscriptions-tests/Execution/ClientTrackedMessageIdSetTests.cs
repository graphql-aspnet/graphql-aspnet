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
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ClientTrackedMessageIdSetTests
    {
        [Test]
        public void DuplicateIdIsRejected()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId(client, "abc123");
            Assert.IsTrue(result);

            result = idSet.ReserveMessageId(client, "abc123");
            Assert.IsFalse(result);
        }

        [Test]
        public void SameIdCanBeAddedToMultipleClients()
        {
            var clientA = new Mock<ISubscriptionClientProxy>().Object;
            var clientB = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId(clientA, "abc123");
            Assert.IsTrue(result);

            result = idSet.ReserveMessageId(clientB, "abc123");
            Assert.IsTrue(result);

            Assert.IsTrue(idSet.Contains(clientA, "abc123"));
            Assert.IsTrue(idSet.Contains(clientB, "abc123"));
        }

        [Test]
        public void Contains_ClientWithoutReservedIdReturnsFalse()
        {
            var clientA = new Mock<ISubscriptionClientProxy>().Object;
            var clientB = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId(clientA, "abc123");
            Assert.IsTrue(result);

            Assert.IsTrue(idSet.Contains(clientA, "abc123"));
            Assert.IsFalse(idSet.Contains(clientB, "abc123"));
        }

        [Test]
        public void ContainsReturnsTrue_WhenIdIsPresent()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId(client, "abc123");

            Assert.IsTrue(result);
            Assert.IsTrue(idSet.Contains(client, "abc123"));
        }

        [Test]
        public void ReleasedIdIsAllowedToBeAdded()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId(client, "abc123");
            Assert.IsTrue(result);

            idSet.ReleaseMessageId(client, "abc123");
            result = idSet.ReserveMessageId(client, "abc123");
            Assert.IsTrue(result);
        }

        [Test]
        public void ReleasedClientDropsAllIds()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId(client, "abc123");
            Assert.IsTrue(result);
            result = idSet.ReserveMessageId(client, "abc1234");
            Assert.IsTrue(result);

            idSet.ReleaseClient(client);
            Assert.IsFalse(idSet.Contains(client, "abc123"));
            Assert.IsFalse(idSet.Contains(client, "abc1234"));
        }
    }
}