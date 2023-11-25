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
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.SubscriptionServer;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Mocks;
    using GraphQL.AspNet.Tests.SubscriptionServer.ClientSubscriptionTestData;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class ClientSubscriptionTests
    {
        [Test]
        public async Task ClientSubscription_FromQueryData_GeneralPropertyCheck()
        {
            var testServer = new TestServerBuilder()
                .AddController<ClientSubscriptionTestController>()
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
            Assert.AreEqual("[type]/Subscription/WatchObjects", sub.Route.Path);
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

            var fakePlan = Substitute.For<IQueryExecutionPlan>();
            var fakeOp = Substitute.For<IExecutableOperation>();

            fakePlan.Operation.Returns(fakeOp);
            fakePlan.Messages.Returns(null as IGraphMessageCollection);
            fakeOp.OperationType.Returns(GraphOperationType.Query);

            var result = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                result.Client,
                GraphQueryData.Empty,
                fakePlan,
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

            var fakePlan = Substitute.For<IQueryExecutionPlan>();
            var fakeOp = Substitute.For<IExecutableOperation>();
            var fakeFieldContext = Substitute.For<IGraphFieldInvocationContext>();
            fakeFieldContext.Field.Returns(null as IGraphField);

            var fakeFieldContexts = Substitute.For<IFieldInvocationContextCollection>();
            fakeFieldContexts[Arg.Any<int>()].Returns(fakeFieldContext);

            fakePlan.Operation.Returns(fakeOp);
            fakePlan.Messages.Returns(null as IGraphMessageCollection);
            fakeOp.OperationType.Returns(GraphOperationType.Subscription);
            fakeOp.FieldContexts.Returns(fakeFieldContexts);

            var result = testServer.CreateSubscriptionClient();

            var sub = new ClientSubscription<GraphSchema>(
                result.Client,
                GraphQueryData.Empty,
                fakePlan,
                "abc123");

            Assert.IsFalse(sub.IsValid);
            Assert.AreEqual(1, sub.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.BAD_REQUEST, sub.Messages[0].Code);
            Assert.IsTrue(sub.Messages.Severity.IsCritical());
        }
    }
}