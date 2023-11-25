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
    using System.Data;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Mocks;
    using GraphQL.AspNet.Tests.Schemas.SchemaTestData;
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
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            SubscriptionEventSchemaMap.ClearCache();

            var schema = new TestServerBuilder<EventMapSchema>()
                .AddSubscriptionServer()
                .AddGraphController<OneFieldMapController>()
                .Build()
                .Schema;

            var map = SubscriptionEventSchemaMap.CreateEventMap(schema);
            var pathName = "[type]/Subscription_OneFieldMap/TestActionMethod";
            var eventName = new SubscriptionEventName(typeof(EventMapSchema), "TestActionMethod");

            Assert.AreEqual(1, map.Count);
            Assert.IsTrue(map.ContainsKey(eventName));
            Assert.IsNotNull(map[eventName]);
            Assert.AreEqual(pathName, map[eventName].Path);
        }

        [Test]
        public void RetrieveFieldPathByName_YieldsCorrectPath()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            SubscriptionEventSchemaMap.ClearCache();

            var schema = new TestServerBuilder<EventMapSchema>()
                .AddSubscriptionServer()
                .AddGraphController<OneFieldMapController>()
                .Build()
                .Schema;

            var map = SubscriptionEventSchemaMap.CreateEventMap(schema);
            var pathName = "[type]/Subscription_OneFieldMap/TestActionMethod";
            var eventName = new SubscriptionEventName(
                typeof(EventMapSchema),
                nameof(OneFieldMapController.TestActionMethod));

            var fieldPath = schema.RetrieveSubscriptionFieldPath(eventName);

            Assert.IsNotNull(fieldPath);
            Assert.AreEqual(pathName, fieldPath.Path);
        }

        [Test]
        public void DuplicateEventName_ThrowsExceptionOnBuild()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            SubscriptionEventSchemaMap.ClearCache();
            var ex = Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var schema = new TestServerBuilder<EventMapSchema>()
                .AddSubscriptionServer()
                .AddGraphController<DuplicateEventNameController>()
                .Build()
                .Schema;
            });

            Assert.IsTrue(ex.Message.Contains("Duplciate Subscription Event Name"));
        }
    }
}