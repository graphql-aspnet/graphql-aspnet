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
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.Subscriptions.Tests.Execution.ExecutionDirectiveTestData;
    using GraphQL.Subscriptions.Tests.Mock;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ExecutionDirectiveTests
    {
        [TestCase("query @recordLocation  { retrieveObject { property1 } }", null, DirectiveLocation.QUERY)]
        [TestCase("mutation @recordLocation  { mutateObject { property1 } }", null, DirectiveLocation.MUTATION)]
        [TestCase("subscription @recordLocation  { onChanged { property1 } }", null, DirectiveLocation.SUBSCRIPTION)]
        [TestCase("query { retrieveObject { property1 @recordLocation } }", null, DirectiveLocation.FIELD)]
        [TestCase("query { retrieveObject { ... frag1 } } fragment frag1 on TwoPropertyObject @recordLocation { property1 } ", null, DirectiveLocation.FRAGMENT_DEFINITION)]
        [TestCase("query { retrieveObject { ... frag1 } } fragment frag1 on TwoPropertyObject { property1 @recordLocation } ", null, DirectiveLocation.FIELD)]
        [TestCase("query { retrieveObject { ... frag1 @recordLocation } } fragment frag1 on TwoPropertyObject { property1 } ", null, DirectiveLocation.FRAGMENT_SPREAD)]
        [TestCase("query { retrieveObject { ... @recordLocation { property1 } } }", null, DirectiveLocation.INLINE_FRAGMENT)]
        [TestCase("query ($id: String! @recordLocation ){ retrieveSingleObject (id: $id) {  property1  } }", @"{ ""id"" : ""bob"" }", DirectiveLocation.VARIABLE_DEFINITION)]
        [TestCase("query ($id: String = \"jane\" @recordLocation ){ retrieveSingleObject (id: $id) {  property1  } }", @"{ ""id"" : ""bob"" }", DirectiveLocation.VARIABLE_DEFINITION)]
        public async Task DirectiveLocationIsAnExpectedValue(string queryText, string variables, DirectiveLocation expectedLocation)
        {
            var directiveInstance = new DirectiveLocationRecorderDirective();

            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddSingleton(directiveInstance);
            serverBuilder.AddGraphController<DirectiveTestController>()
                   .AddSubscriptionServer()
                   .AddDirective<DirectiveLocationRecorderDirective>();

            var server = serverBuilder.Build();

            var subClient = server.CreateSubscriptionClient();

            var builder = server.CreateSubcriptionContextBuilder(
                subClient.Client,
                subClient.ServiceProvider,
                subClient.SecurityContext)
                .AddQueryText(queryText);

            if (variables != null)
                builder.AddVariableData(variables);

            var id = "bob";
            var context = builder.Build(id);
            await server.ExecuteQuery(context);

            Assert.AreEqual(expectedLocation, directiveInstance.RecordedLocation);
        }

        [Test]
        public async Task OperationRequestIsCorrectlySetOnInvokedExecutionDirective()
        {
            var queryText = "query  { retrieveObject @operationRequestCheck { property1 } }";

            var directiveInstance = new OperationRequestCheckDirective();
            directiveInstance.ExpectedQueryText = queryText;

            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddSingleton(directiveInstance);
            serverBuilder.AddGraphController<DirectiveTestController>()
                   .AddSubscriptionServer()
                   .AddDirective<OperationRequestCheckDirective>();

            var server = serverBuilder.Build();

            var subClient = server.CreateSubscriptionClient();

            var builder = server.CreateSubcriptionContextBuilder(
                subClient.Client,
                subClient.ServiceProvider,
                subClient.SecurityContext)
                .AddQueryText(queryText);

            var id = "bob";
            var context = builder.Build(id);
            await server.ExecuteQuery(context);

            Assert.IsTrue(directiveInstance.OperationRequestReceived);
        }
    }
}