// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.SubscriptionServer
{
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Mocks;
    using GraphQL.Subscriptions.Tests.SubscriptionServer.ClientSubscriptionTestData;
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

            var queryPlan = await testServer.CreateQueryPlan("subscription { watchObjects { property1 property2  }} ");

            Assert.IsNotNull(queryPlan.Operation);
            Assert.AreEqual(0, queryPlan.Messages.Count);

            var field = queryPlan.Operation.FieldContexts[0].Field;
            var name = field.GetType().FullName;

            var result = testServer.CreateSubscriptionClient();

            var queryData = new GraphQueryData();

            var sub = new ClientSubscription<GraphSchema>(
                result.Client,
                queryData,
                queryPlan,
                "abc123");

            Assert.IsTrue(sub.IsValid);
            Assert.AreEqual("[subscription]/WatchObjects", sub.Route.Path);
            Assert.AreEqual("abc123", sub.Id);
            Assert.AreEqual(field, sub.Field);
            Assert.AreEqual(result.Client, sub.Client);
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
            var fakeOp = new Mock<IExecutableOperation>();

            fakePlan.Setup(x => x.Operation).Returns(fakeOp.Object);
            fakeOp.Setup(x => x.OperationType).Returns(GraphOperationType.Query);

            var result = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                result.Client,
                GraphQueryData.Empty,
                fakePlan.Object,
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
            var fakeOp = new Mock<IExecutableOperation>();
            var fakeFieldContext = new Mock<IGraphFieldInvocationContext>();
            fakeFieldContext.Setup(x => x.Field).Returns(null as IGraphField);

            var fakeFieldContexts = new Mock<IFieldInvocationContextCollection>();
            fakeFieldContexts.Setup(x => x[It.IsAny<int>()]).Returns(fakeFieldContext.Object);

            fakePlan.Setup(x => x.Operation).Returns(fakeOp.Object);
            fakeOp.Setup(x => x.OperationType).Returns(GraphOperationType.Subscription);
            fakeOp.Setup(x => x.FieldContexts).Returns(fakeFieldContexts.Object);

            var result = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                result.Client,
                GraphQueryData.Empty,
                fakePlan.Object,
                "abc123");

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.BAD_REQUEST, sub.Messages[0].Code);
            Assert.IsTrue(sub.Messages.Severity.IsCritical());
        }
    }
}