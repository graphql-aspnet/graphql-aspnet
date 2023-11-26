// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Execution.ExecutionDirectiveTestData;
    using GraphQL.AspNet.Tests.Execution.SubscriptionQueryExecutionData;
    using GraphQL.AspNet.Tests.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionQueryExecutionDirectiveTests
    {
        [Test]
        public async Task ExecutionOfAQueryPlan_WithValidDefaultObject_forSubscription_YieldsResult()
        {
            var server = new TestServerBuilder()
                        .AddController<SubQueryController>()
                        .AddDirective<ToLowerDirective>()
                        .AddSubscriptionServer()
                        .Build();

            var template = GraphQLTemplateHelper.CreateActionMethodTemplate<SubQueryController>(nameof(SubQueryController.RetrieveObject));

            var sourceObject = new TwoPropertyObject()
            {
                Property1 = "TEST DATA SUPPLIED IN UPPER CASE",
                Property2 = 5,
            };

            // Add a default value for the "retrieveObject" method, which is a subscription action
            // this mimics recieving an subscription event data source and executing the default, normal pipeline
            // to produce a final result that can be returned along the client connection
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                        subscription {
                            subscriptionData {
                                retrieveObject {
                                    property1 @toLower
                                }
                            }
                        }")
                .AddDefaultValue(template.Route, sourceObject);

            var result = await server.RenderResult(builder);
            var expectedOutput =
                @"{
                    ""data"" : {
                        ""subscriptionData"" : {
                            ""retrieveObject"" : {
                                ""property1"" : ""test data supplied in upper case""
                            }
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}