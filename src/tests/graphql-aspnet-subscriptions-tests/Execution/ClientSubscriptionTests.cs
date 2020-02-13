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
    using System.Linq;
    using System.Threading.Tasks;
    using Castle.DynamicProxy.Generators;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Execution.ClientSubscriptionTestData;
    using GraphQL.Subscriptions.Tests.TestServerHelpers;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
    using NUnit.Framework;
    using NUnit.Framework.Internal;

    [TestFixture]
    public class ClientSubscriptionTests
    {
        [Test]
        public async Task ClientSubscription_FromQueryData_GeneralPropertyCheck()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptions()
                .Build();

            var schema = testServer.Schema;
            var subServer = testServer.RetrieveSubscriptionServer();
            var queryPlan = await testServer.CreateQueryPlan("subscription { watchObjects { property1 property2  }} ");

            Assert.AreEqual(1, queryPlan.Operations.Count);
            Assert.AreEqual(0, queryPlan.Messages.Count);

            var field = queryPlan.Operations.Values.First().FieldContexts[0].Field;
            var name = field.GetType().FullName;

            (var socketClient, var testClient) = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                "abc123",
                queryPlan);

            Assert.IsTrue(sub.IsValid);
            Assert.AreEqual("[subscription]/WatchObjects", sub.Route.Path);
            Assert.AreEqual("abc123", sub.ClientProvidedId);
            Assert.AreEqual(field, sub.Field);
            Assert.AreEqual(testClient, sub.Client);
        }

        [Test]
        public async Task ClientSubscription_ReferencedOperationIsNotASubscription_InvalidSub()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptions()
                .Build();

            var schema = testServer.Schema;
            var subServer = testServer.RetrieveSubscriptionServer();
            var queryPlan = await testServer.CreateQueryPlan("subscription Operation1{ watchObjects { property1 property2  }} " +
                "query Operation2{ retrieveObject(id: 5){ property1 } } ");

            Assert.AreEqual(2, queryPlan.Operations.Count);
            Assert.AreEqual(0, queryPlan.Messages.Count);

            (var socketClient, var testClient) = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                "abc123",
                queryPlan,
                "Operation2");

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, sub.Messages.Severity);
        }

        [Test]
        public void ClientSubscription_NoQueryPlan_InvalidSub()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptions()
                .Build();

            (var socketClient, var testClient) = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                "abc123",
                null);

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, sub.Messages.Severity);
        }

        [Test]
        public async Task ClientSubscription_NamedOperationNotFound_InvalidSub()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptions()
                .Build();

            var schema = testServer.Schema;
            var subServer = testServer.RetrieveSubscriptionServer();

            (var socketClient, var testClient) = testServer.CreateSubscriptionClient();

            // two operations in the query
            var queryPlan = await testServer.CreateQueryPlan(
                "subscription DoThings{ watchObjects { property1 property2 } } " +
                "subscription DoOtherThings{ watchObjects { property1 } }");

            Assert.AreEqual(2, queryPlan.Operations.Count);
            Assert.AreEqual(0, queryPlan.Messages.Count);

            var field = queryPlan.Operations.Values.First().FieldContexts[0].Field;
            var name = field.GetType().FullName;

            // create subscription against a non existant operation
            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                "abc123",
                queryPlan,
                "wrongOperationname");

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, sub.Messages.Severity);
            Assert.IsTrue(sub.Messages[0].Message.Contains("wrongOperationname"));
        }
    }
}