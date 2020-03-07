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
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
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
                .AddSubscriptionServer()
                .Build();

            var schema = testServer.Schema;
            var subServer = testServer.RetrieveSubscriptionServer();
            var queryPlan = await testServer.CreateQueryPlan("subscription { watchObjects { property1 property2  }} ");

            Assert.AreEqual(1, queryPlan.Operations.Count);
            Assert.AreEqual(0, queryPlan.Messages.Count);

            var field = queryPlan.Operations.Values.First().FieldContexts[0].Field;
            var name = field.GetType().FullName;

            (var socketClient, var testClient) = await testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                GraphQueryData.Empty,
                queryPlan,
                queryPlan.Operations.First().Value,
                "abc123");

            Assert.IsTrue(sub.IsValid);
            Assert.AreEqual("[subscription]/WatchObjects", sub.Route.Path);
            Assert.AreEqual("abc123", sub.Id);
            Assert.AreEqual(field, sub.Field);
            Assert.AreEqual(testClient, sub.Client);
        }
    }
}