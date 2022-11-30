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
    using GraphQL.Subscriptions.Tests.Mock;
    using NuGet.Frameworks;
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

            var template = TemplateHelper.CreateActionMethodTemplate<SubQueryController>(nameof(SubQueryController.SkipEventMethod));

            var sourceObject = new TwoPropertyObject()
            {
                Property1 = "testA",
                Property2 = 5,
            };

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("subscription  { subscriptionData {  skipEventMethod { property1 } } }")
                .AddDefaultValue(template.Route, sourceObject);

            var context = builder.Build();
            await server.ExecuteQuery(context);

            Assert.IsTrue(context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.SKIP_EVENT));
        }

        [Test]
        public async Task ExecutionOfASubscription_ThatReturnsSkipAndCompleteActionResult_AddsKeysToCollection()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var template = TemplateHelper.CreateActionMethodTemplate<SubQueryController>(nameof(SubQueryController.SkipEventAndCompleteMethod));

            var sourceObject = new TwoPropertyObject()
            {
                Property1 = "testA",
                Property2 = 5,
            };

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("subscription  { subscriptionData {  skipEventAndCompleteMethod { property1 } } }")
                .AddDefaultValue(template.Route, sourceObject);

            var context = builder.Build();
            await server.ExecuteQuery(context);

            Assert.IsTrue(context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.SKIP_EVENT));
            Assert.IsTrue(context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.COMPLETE_SUBSCRIPTION));
        }

        [Test]
        public async Task ExecutionOfASubscription_ThatReturnsACompleteActionResult_AddsKeyToCollection_AndRendersResult()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var template = TemplateHelper.CreateActionMethodTemplate<SubQueryController>(nameof(SubQueryController.CompleteMethod));

            var sourceObject = new TwoPropertyObject()
            {
                Property1 = "testA",
                Property2 = 5,
            };

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("subscription  { subscriptionData {  completeMethod { property1 } } }")
                .AddDefaultValue(template.Route, sourceObject);

            var expectedResult = @"
            {
                ""data"": {
                ""subscriptionData"": {
                    ""completeMethod"": {
                    ""property1"": ""testA""
                    }
                }
                }
            }";

            var context = builder.Build();
            var result = await server.RenderResult(context);

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
            Assert.IsTrue(context.Session.Items.ContainsKey(SubscriptionConstants.ContextDataKeys.COMPLETE_SUBSCRIPTION));
        }

        [Test]
        public async Task ExecutionOfQuery_WithSkipEventResult_AddsError()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { normalQueryWithSkipEvent { property1 } } ");

            var context = builder.Build();
            await server.ExecuteQuery(context);

            Assert.AreEqual(1, context.Messages.Count);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.IsNull(context.Result.Data);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ACTION_RESULT, context.Messages[0].Code);
        }

        [Test]
        public async Task ExecutionOfQuery_WithCompleteEventResult_AddsError()
        {
            var server = new TestServerBuilder()
                        .AddType<SubQueryController>()
                        .AddSubscriptionServer()
                        .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query  { normalQueryWithComplete { property1 } } ");

            var context = builder.Build();
            await server.ExecuteQuery(context);

            Assert.AreEqual(1, context.Messages.Count);
            Assert.IsFalse(context.Messages.IsSucessful);
            Assert.IsNull(context.Result.Data);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ACTION_RESULT, context.Messages[0].Code);
        }
    }
}