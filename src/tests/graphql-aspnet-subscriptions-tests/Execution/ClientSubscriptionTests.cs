﻿// *************************************************************
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
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Subscriptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Execution.ClientSubscriptionTestData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using Microsoft.VisualStudio.TestPlatform.TestExecutor;
    using Moq;
    using NUnit.Framework;

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
            var queryPlan = await testServer.CreateQueryPlan("subscription { watchObjects { property1 property2  }} ");

            Assert.IsNotNull(queryPlan.Operation);
            Assert.AreEqual(0, queryPlan.Messages.Count);

            var field = queryPlan.Operation.FieldContexts[0].Field;
            var name = field.GetType().FullName;

            var clientResult = testServer.CreateSubscriptionClient();

            var queryData = new GraphQueryData();

            var sub = new ClientSubscription<GraphSchema>(
                clientResult.Client,
                queryData,
                queryPlan,
                queryPlan.Operation,
                "abc123");

            Assert.IsTrue(sub.IsValid);
            Assert.AreEqual("[subscription]/WatchObjects", sub.Route.Path);
            Assert.AreEqual("abc123", sub.Id);
            Assert.AreEqual(field, sub.Field);
            Assert.AreEqual(clientResult.Client, sub.Client);
            Assert.AreEqual(queryData, sub.QueryData);
        }

        [Test]
        public void ClientSubscription_NotASubscriptionOperation_ReturnsError()
        {
            var testServer = new TestServerBuilder()
                .AddGraphController<ClientSubscriptionTestController>()
                .AddSubscriptionServer()
                .Build();

            var fakePlan = new Mock<IGraphQueryPlan>();
            var fakeOp = new Mock<IGraphFieldExecutableOperation>();

            fakeOp.Setup(x => x.OperationType).Returns(GraphOperationType.Query);

            var testClient = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient.Client,
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
        public void ClientSubscription_NoFieldContextFound_ReturnsError()
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

            fakeOp.Setup(x => x.OperationType).Returns(GraphOperationType.Subscription);
            fakeOp.Setup(x => x.FieldContexts).Returns(fakeFieldContexts.Object);

            var testClient = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                testClient.Client,
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