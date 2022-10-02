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
    using GraphQL.Subscriptions.Tests.Mocks;
    using GraphQL.Subscriptions.Tests.Schemas.SchemaTestData;
    using NUnit.Framework;

    [TestFixture]
    public class SchemaSubscriptionEventMapTests
    {
        public class EventMapSchema : GraphSchema
        {
        }

        [Test]
        public void MapOfFieldWithNoEventName_RendersOneItem()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();
            SchemaSubscriptionEventMap.ClearCache();

            var schema = new TestServerBuilder<EventMapSchema>()
                .AddSubscriptionServer()
                .AddGraphController<OneFieldMapController>()
                .Build()
                .Schema;

            var map = SchemaSubscriptionEventMap.CreateEventMap(schema);
            var pathName = "[subscription]/OneFieldMap/TestActionMethod";
            var eventName = new SubscriptionEventName(typeof(EventMapSchema), "TestActionMethod");

            Assert.AreEqual(1, map.Count);
            Assert.IsTrue(map.ContainsKey(eventName));
            Assert.IsNotNull(map[eventName]);
            Assert.AreEqual(pathName, map[eventName].Path);
        }

        [Test]
        public void RetrieveFieldPathByName_YieldsCorrectPath()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();
            SchemaSubscriptionEventMap.ClearCache();

            var schema = new TestServerBuilder<EventMapSchema>()
                .AddSubscriptionServer()
                .AddGraphController<OneFieldMapController>()
                .Build()
                .Schema;

            var map = SchemaSubscriptionEventMap.CreateEventMap(schema);
            var pathName = "[subscription]/OneFieldMap/TestActionMethod";
            var eventName = new SubscriptionEventName(
                typeof(EventMapSchema),
                nameof(OneFieldMapController.TestActionMethod));

            var fieldPath = schema.RetrieveSubscriptionFieldPath(eventName);

            Assert.IsNotNull(fieldPath);
            Assert.AreEqual(pathName, fieldPath.Path);
        }

        [Test]
        public void DuplicateEventName_ThrowsException()
        {
            using var restorePoint = new GraphQLProviderRestorePoint();
            SchemaSubscriptionEventMap.ClearCache();

            var schema = new TestServerBuilder<EventMapSchema>()
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