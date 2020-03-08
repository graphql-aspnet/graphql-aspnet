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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Castle.DynamicProxy.Generators;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Execution.ClientSubscriptionTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
    using Moq;
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

            var queryData = new GraphQueryData();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                queryData,
                queryPlan,
                queryPlan.Operations.First().Value,
                "abc123");

            Assert.IsTrue(sub.IsValid);
            Assert.AreEqual("[subscription]/WatchObjects", sub.Route.Path);
            Assert.AreEqual("abc123", sub.Id);
            Assert.AreEqual(field, sub.Field);
            Assert.AreEqual(testClient, sub.Client);
            Assert.AreEqual(queryData, sub.QueryData);
        }

        [Test]
        public async Task ClientSubscription_NotASubscriptionOperation_ReturnsError()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptionServer()
                .Build();

            var fakePlan = new Mock<IGraphQueryPlan>();
            var fakeOp = new Mock<IGraphFieldExecutableOperation>();

            fakeOp.Setup(x => x.OperationType).Returns(GraphCollection.Query);

            (var socketClient, var testClient) = await testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                GraphQueryData.Empty,
                fakePlan.Object,
                fakeOp.Object,
                "abc123");

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.BAD_REQUEST, sub.Messages[0].Code);
            Assert.IsTrue(sub.Messages.Severity.IsCritical());
        }

        [Test]
        public async Task ClientSubscription_NoFieldContextFound_ReturnsError()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptionServer()
                .Build();

            var fakePlan = new Mock<IGraphQueryPlan>();
            var fakeOp = new Mock<IGraphFieldExecutableOperation>();
            var fakeFieldContext = new Mock<IGraphFieldInvocationContext>();
            fakeFieldContext.Setup(x => x.Field).Returns(null as IGraphField);

            var fakeFieldContexts = new Mock<IFieldInvocationContextCollection>();
            fakeFieldContexts.Setup(x => x[It.IsAny<int>()]).Returns(fakeFieldContext.Object);

            fakeOp.Setup(x => x.OperationType).Returns(GraphCollection.Subscription);
            fakeOp.Setup(x => x.FieldContexts).Returns(fakeFieldContexts.Object);

            (var socketClient, var testClient) = await testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient,
                GraphQueryData.Empty,
                fakePlan.Object,
                fakeOp.Object,
                "abc123");

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.BAD_REQUEST, sub.Messages[0].Code);
            Assert.IsTrue(sub.Messages.Severity.IsCritical());
        }
    }
}