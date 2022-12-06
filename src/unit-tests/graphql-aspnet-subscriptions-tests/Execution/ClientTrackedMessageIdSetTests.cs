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

            var result = idSet.ReserveMessageId("abc123");
            Assert.IsTrue(result);

            result = idSet.ReserveMessageId("abc123");
            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsReturnsTrue_WhenIdIsPresent()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId("abc123");

            Assert.IsTrue(result);
            Assert.IsTrue(idSet.Contains("abc123"));
        }

        [Test]
        public void ReleasedIdIsAllowedToBeAdded()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId("abc123");
            Assert.IsTrue(result);

            idSet.ReleaseMessageId("abc123");
            result = idSet.ReserveMessageId("abc123");
            Assert.IsTrue(result);
        }

        [Test]
        public void ClearDropsAllIds()
        {
            var client = new Mock<ISubscriptionClientProxy>().Object;
            var idSet = new ClientTrackedMessageIdSet();

            var result = idSet.ReserveMessageId("abc123");
            Assert.IsTrue(result);
            result = idSet.ReserveMessageId("abc1234");
            Assert.IsTrue(result);

            idSet.Clear();
            Assert.IsFalse(idSet.Contains("abc123"));
            Assert.IsFalse(idSet.Contains("abc1234"));
        }
    }
}