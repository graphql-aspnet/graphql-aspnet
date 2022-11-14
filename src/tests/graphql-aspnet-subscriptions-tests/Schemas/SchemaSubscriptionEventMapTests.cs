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
    using GraphQL.AspNet.Execution.Exceptions;
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
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
            Assert.IsTrue(map.ContainsKey(eventName.SchemaQualifiedEventName));
            Assert.IsNotNull(map[eventName.SchemaQualifiedEventName]);
            Assert.AreEqual(pathName, map[eventName.SchemaQualifiedEventName].Path);
        }

        [Test]
        public void RetrieveFieldPathByName_YieldsCorrectPath()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
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
        public void DuplicateEventName_ThrowsExceptionOnBuild()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint();
            SchemaSubscriptionEventMap.ClearCache();
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