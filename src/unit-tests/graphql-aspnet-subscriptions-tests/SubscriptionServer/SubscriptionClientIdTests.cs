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
    using GraphQL.AspNet.SubscriptionServer;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionClientIdTests
    {
        [Test]
        public void NewClientId_CreatesANewId()
        {
            var id = SubscriptionClientId.NewClientId();

            Assert.AreNotEqual(Guid.Empty.ToString(), id.ToString());
        }

        [Test]
        public void Empty()
        {
            var id = SubscriptionClientId.Empty;
            Assert.AreEqual(Guid.Empty.ToString(), id.ToString());
        }

        [Test]
        public void EqualalityOperator()
        {
            var g = Guid.NewGuid();
            var id = SubscriptionClientId.FromGuid(g);
            var otherId = SubscriptionClientId.FromGuid(g);

            Assert.IsTrue(id == otherId);
        }

        [Test]
        public void InequalityOperator()
        {
            var id = SubscriptionClientId.NewClientId();
            var otherId = SubscriptionClientId.NewClientId();

            Assert.IsTrue(id != otherId);
        }
    }
}