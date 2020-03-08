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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Execution.SubscriptionQueryExecutionData;
    using GraphQL.Subscriptions.Tests.TestServerExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionCreationTests
    {
        [Test]
        public async Task SubscriptionExecution_YieldsSubscriptionOnContext()
        {
            var server = new TestServerBuilder()
                        .AddGraphType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            (var socketClient, var subClient) = await server.CreateSubscriptionClient();

            var builder = server.CreateSubcriptionContextBuilder(subClient)
                .AddQueryText("subscription  { subscriptionData {  retrieveObject { property1 } } }");

            var id = "bob";
            var context = builder.Build(id);
            await server.ExecuteQuery(context);

            Assert.IsTrue(context.IsSubscriptionOperation);
            Assert.IsNotNull(context.Subscription);

            var createdSub = context.Subscription;
            Assert.IsTrue(createdSub.IsValid);
            Assert.AreEqual("[subscription]/subscriptionData/RetrieveObject", createdSub.Field.Route.Path);
            Assert.AreEqual(id, createdSub.Id);
            Assert.AreEqual(0, createdSub.Messages.Count);
            Assert.AreEqual(subClient, createdSub.Client);
            Assert.AreEqual(GraphCollection.Subscription, createdSub.QueryOperation.OperationType);
            Assert.IsNull(context.Result);
        }

        [Test]
        public async Task SubscriptionExecution_InvalidField_YieldsMessages()
        {
            var server = new TestServerBuilder()
                        .AddGraphType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            (var socketClient, var subClient) = await server.CreateSubscriptionClient();

            var builder = server.CreateSubcriptionContextBuilder(subClient)
                .AddQueryText("subscription  { subscriptionData {  notAField { property1 } } }");

            var id = "bob";
            var context = builder.Build(id);
            await server.ExecuteQuery(context);

            Assert.IsFalse(context.IsSubscriptionOperation);
            Assert.AreEqual(1, context.Messages.Count);
        }

        [Test]
        public async Task SubscriptionExecution_WhenNotASubscription_BypassesCreation()
        {
            var server = new TestServerBuilder()
                        .AddGraphType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            (var socketClient, var subClient) = await server.CreateSubscriptionClient();

            // Add a default value for the "retrieveObject" method, which is a subscription action
            // this mimics recieving an subscription event data source and executing the default, normal pipeline
            // to produce a final result that can be returned along the client connection
            var builder = server.CreateSubcriptionContextBuilder(subClient)
                .AddQueryText("query  { subscriptionData {  queryRetrieveObject { property1 } } }");

            var id = "bob";
            var context = builder.Build(id);
            await server.ExecuteQuery(context);

            Assert.IsFalse(context.IsSubscriptionOperation);
            Assert.IsNull(context.Subscription);
            Assert.IsNotNull(context.Result);
        }
    }
}