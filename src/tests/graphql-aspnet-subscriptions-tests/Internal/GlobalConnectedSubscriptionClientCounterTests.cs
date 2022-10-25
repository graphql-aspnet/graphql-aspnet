// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Internal
{
    using System;
    using GraphQL.AspNet.Internal;
    using NUnit.Framework;

    [TestFixture]
    public class GlobalConnectedSubscriptionClientCounterTests
    {
        [TestCase(0)]
        [TestCase(-45)]
        public void WhenCreatdWithLessThan1Max_ExceptionThrown(int maxAllowed)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var instance = new GlobalConnectedSubscriptionClientCounter(maxAllowed);
            });
        }

        [Test]
        public void IndicatingCount_Increases()
        {
            var instance = new GlobalConnectedSubscriptionClientCounter(2);

            var result = instance.IncreaseCount();
            Assert.AreEqual(1, instance.TotalConnectedClients);
        }

        [Test]
        public void IndicatingCount_Decreases()
        {
            var instance = new GlobalConnectedSubscriptionClientCounter(2);

            var result = instance.IncreaseCount();
            instance.DecreaseCount();
            Assert.AreEqual(0, instance.TotalConnectedClients);
        }

        [Test]
        public void IndicatingCount_DoesntDropBelowZero()
        {
            var instance = new GlobalConnectedSubscriptionClientCounter(2);

            instance.DecreaseCount();
            Assert.AreEqual(0, instance.TotalConnectedClients);
        }

        [Test]
        public void WhenMaxAllowedMet_NextIsDenied()
        {
            var instance = new GlobalConnectedSubscriptionClientCounter(2);

            var result = instance.IncreaseCount();
            Assert.IsTrue(result);

            result = instance.IncreaseCount();
            Assert.IsTrue(result);

            result = instance.IncreaseCount();
            Assert.IsFalse(result);
        }
    }
}