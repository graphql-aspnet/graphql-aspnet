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
    using GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class GeneralQueryExecutionTests3
    {
        [Test]
        public async Task Record_asInputObject_RendersObjectCorrectly()
        {
            var server = new TestServerBuilder()
                  .AddGraphQL(o =>
                  {
                      o.AddType<RecordAsInputObjectController>();
                  })
                  .Build();

            // totalPeople exists on  base controller
            // totalEmployees exists on  the added EmployeeController
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(@"query {
                    retrieveValue(record: {property1: 23})
                 }");

            var expectedOutput =
                @"{
                    ""data"": {
                       ""retrieveValue"" :  23
                    }
                  }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}