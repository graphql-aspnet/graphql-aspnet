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
    using NUnit.Framework;

    public class SubscriptionEventTests
    {
        [Test]
        public void GeneralPropertyCheck()
        {
            var eventData = new SubscriptionEvent()
            {
                Data = new object(),
                DataTypeName = "dataType",
                SchemaTypeName = "schemaType",
                EventName = "eventName",
            };

            Assert.AreEqual(new SubscriptionEventName("schemaType", "eventName"), eventData.ToSubscriptionEventName());
        }
    }
}