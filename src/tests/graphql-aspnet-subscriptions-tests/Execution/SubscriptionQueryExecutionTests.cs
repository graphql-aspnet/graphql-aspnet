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
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using GraphQL.AspNet;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.Subscriptions.Tests.Execution.SubscriptionQueryExecutionData;
    using GraphQL.Subscriptions.Tests.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionQueryExecutionTests
    {
        [Test]
        public async Task ExecutionOfAQueryPlan_WithValidDefaultObject_forSubscription_YieldsResult()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var template = TemplateHelper.CreateActionMethodTemplate<SubQueryController>(nameof(SubQueryController.RetrieveObject));

            var sourceObject = new TwoPropertyObject()
            {
                Property1 = "testA",
                Property2 = 5,
            };

            // Add a default value for the "retrieveObject" method, which is a subscription action
            // this mimics recieving an subscription event data source and executing the default, normal pipeline
            // to produce a final result that can be returned along the client connection
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("subscription  { subscriptionData {  retrieveObject { property1 } } }")
                .AddDefaultValue(template.Route, sourceObject);

            var result = await server.RenderResult(builder);
            var expectedOutput =
                @"{
                    ""data"" : {
                        ""subscriptionData"" : {
                            ""retrieveObject"" : {
                                ""property1"" : ""testA""
                            }
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task ExecutionOfASubscription_ThatReturnsSkipActionResult_AddsKeyToCollection()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var template = TemplateHelper.CreateActionMethodTemplate<SubQueryController>(nameof(SubQueryController.RetrieveObjectButSkipped));

            var sourceObject = new TwoPropertyObject()
            {
                Property1 = "testA",
                Property2 = 5,
            };

            // Add a default value for the "retrieveObject" method, which is a subscription action
            // this mimics recieving an subscription event data source and executing the default, normal pipeline
            // to produce a final result that can be returned along the client connection
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("subscription  { subscriptionData {  retrieveObjectButSkipped { property1 } } }")
                .AddDefaultValue(template.Route, sourceObject);

            var context = builder.Build();
            await server.ExecuteQuery(context);

            Assert.IsTrue(context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.SKIP_EVENT));
        }
    }
}