// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Configuration
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionServerOptionsTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            // ensure no funny business with retrieving set values
            var options = new SubscriptionServerOptions<GraphSchema>();

            Assert.IsFalse(options.DisableDefaultRoute);
            options.DisableDefaultRoute = true;
            Assert.IsTrue(options.DisableDefaultRoute);

            options.Route = "bob";
            Assert.AreEqual("bob", options.Route);

            options.MessageBufferSize = 15;
            Assert.AreEqual(15, options.MessageBufferSize);
        }
    }
}