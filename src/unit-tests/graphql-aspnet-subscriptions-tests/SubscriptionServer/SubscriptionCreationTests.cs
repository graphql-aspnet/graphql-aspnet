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
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Execution.SubscriptionQueryExecutionData;
    using GraphQL.AspNet.Tests.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionCreationTests
    {
        [Test]
        public async Task SubscriptionExecution_YieldsSubscriptionOnContext()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var result = server.CreateSubscriptionClient();

            var builder = server.CreateSubcriptionContextBuilder(
                result.Client,
                result.ServiceProvider,
                result.SecurityContext)
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
            Assert.AreEqual(result.Client, createdSub.Client);
            Assert.AreEqual(GraphOperationType.Subscription, createdSub.QueryOperation.OperationType);
            Assert.IsNull(context.Result);
        }

        [Test]
        public async Task SubscriptionExecution_InvalidField_YieldsMessages()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var result = server.CreateSubscriptionClient();

            var builder = server.CreateSubcriptionContextBuilder(
                result.Client,
                result.ServiceProvider,
                result.SecurityContext)
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
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var result = server.CreateSubscriptionClient();

            // Add a default value for the "retrieveObject" method, which is a subscription action
            // this mimics recieving an subscription event data source and executing the default, normal pipeline
            // to produce a final result that can be returned along the client connection
            var builder = server.CreateSubcriptionContextBuilder(
                result.Client,
                result.ServiceProvider,
                result.SecurityContext)
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