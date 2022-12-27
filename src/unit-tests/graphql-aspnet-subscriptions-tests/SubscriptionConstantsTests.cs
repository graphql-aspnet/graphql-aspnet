// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionConstantsTests
    {
        [Test]
        public void DefaultQueueAlertSettings_AreNotNull()
        {
            var settings = SubscriptionConstants.Alerts.DefaultDispatchQueueAlertSettings;
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.AlertThresholds.Count > 0);
            Assert.IsTrue(settings.AlertThresholds[0].SubscriptionEventCountThreshold > 0);
        }
    }
}