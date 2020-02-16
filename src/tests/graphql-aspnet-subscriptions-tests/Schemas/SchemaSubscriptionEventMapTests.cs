// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Schemas
{
    using System.Data;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Schemas.SchemaTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaSubscriptionEventMapTests
    {
        [Test]
        public void MapOfFieldWithNoEventName_RendersOneItem()
        {
            var schema = new TestServerBuilder()
                .AddSubscriptionServer()
                .AddGraphController<OneFieldMapController>()
                .Build()
                .Schema;

            var map = SchemaSubscriptionEventMap.CreateEventMap(schema);
            var pathName = "[subscription]/OneFieldMap/TestActionMethod";
            var eventName = new SubscriptionEventName(typeof(GraphSchema), pathName);

            Assert.AreEqual(1, map.Count);
            Assert.IsTrue(map.ContainsKey(eventName));
            Assert.IsNotNull(map[eventName]);
            Assert.AreEqual(pathName, map[eventName].Path);
        }

        [Test]
        public void MapOfFieldWithEventName_RendersTwoItem()
        {
            var schema = new TestServerBuilder()
                .AddSubscriptionServer()
                .AddGraphController<OneFieldMapWithEventNameController>()
                .Build()
                .Schema;

            var map = SchemaSubscriptionEventMap.CreateEventMap(schema);
            var pathName = "[subscription]/OneFieldMapWithEventName/TestActionMethod";

            var pathEventName = new SubscriptionEventName(typeof(GraphSchema), pathName);
            var eventName = new SubscriptionEventName(typeof(GraphSchema), "shortTestName");

            Assert.AreEqual(2, map.Count);
            Assert.IsTrue(map.ContainsKey(eventName));
            Assert.IsNotNull(map[eventName]);
            Assert.AreEqual(pathName, map[eventName].Path);

            Assert.IsTrue(map.ContainsKey(eventName));
            Assert.IsNotNull(map[eventName]);
            Assert.AreEqual(pathName, map[eventName].Path);

            // ensure both items reference the same object
            Assert.AreSame(map[pathEventName], map[eventName]);
        }

        [Test]
        public void DuplicateEventName_ThrowsException()
        {
            var schema = new TestServerBuilder()
                .AddSubscriptionServer()
                .AddGraphController<DuplicateEventNameController>()
                .Build()
                .Schema;

            Assert.Throws<DuplicateNameException>(() =>
            {
                var map = SchemaSubscriptionEventMap.CreateEventMap(schema);
            });
        }
    }
}