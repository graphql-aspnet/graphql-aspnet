// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeFieldGeneralTests
    {
        [Test]
        public void General_SubscriptionField_IsRegistered()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapSubscription("field1", (TwoPropertyObject source, int param1) => source);
                })
                .AddSubscriptionServer()
                .Build();

            var operation = server.Schema.Operations[GraphOperationType.Subscription];
            var field = operation.Fields.FindField("field1") as ISubscriptionGraphField;
            Assert.IsNotNull(field);
            Assert.AreEqual("field1", field.Name);
            Assert.AreEqual("field1", field.EventName);
        }

        [Test]
        public void General_SubscriptionField_WithCustomEventName_IsRegistered()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapSubscription("field1", (TwoPropertyObject source, int param1) => source)
                    .WithEventName("myEvent");
                })
                .AddSubscriptionServer()
                .Build();

            var operation = server.Schema.Operations[GraphOperationType.Subscription];
            var field = operation.Fields.FindField("field1") as ISubscriptionGraphField;
            Assert.IsNotNull(field);
            Assert.AreEqual("field1", field.Name);
            Assert.AreEqual("myEvent", field.EventName);
        }

        [Test]
        public void General_SubscriptionField_WithCustomInternalName_IsRegistered()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapSubscription("field1", (TwoPropertyObject source, int param1) => source)
                    .WithInternalName("myInternalName");
                })
                .AddSubscriptionServer()
                .Build();

            var operation = server.Schema.Operations[GraphOperationType.Subscription];
            var field = operation.Fields.FindField("field1") as ISubscriptionGraphField;
            Assert.IsNotNull(field);
            Assert.AreEqual("field1", field.Name);
            Assert.AreEqual("myInternalName", field.InternalName);
        }

        [Test]
        public void AddSubscriptionField_WithoutRegistereingSubscriptionServer_ThrowsException()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapSubscription("field1", (TwoPropertyObject source, int param1) => source);
                });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                serverBuilder.Build();
            });
        }

        [Test]
        public void AddSubscriptionField_NoApplicableSourceObject_ThrownException()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapSubscription("field1", () => 0);
                })
                .AddSubscriptionServer();

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                serverBuilder.Build();
            });
        }

        [Test]
        public void AddSubscriptionField_ViaBuilder_BeforeSubscriptionServer_RendersField()
        {
            using var restorePoint = new GraphQLGlobalSubscriptionRestorePoint();
            var serverBuilder = new TestServerBuilder();

            var schemaBuilder = serverBuilder.AddGraphQL();
            schemaBuilder.MapSubscription("field1", (int arg1) => 0);
            schemaBuilder.AddSubscriptions();

            var server = serverBuilder.Build();

            var operation = server.Schema.Operations[GraphOperationType.Subscription];
            var field = operation.Fields.FindField("field1") as ISubscriptionGraphField;
            Assert.IsNotNull(field);
        }
    }
}