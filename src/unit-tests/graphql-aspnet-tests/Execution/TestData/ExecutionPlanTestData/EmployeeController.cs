// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphRoute("employees")]
    public class EmployeeController : PersonController
    {
        [Query]
        public int TotalEmployees()
        {
            return 5;
        }
    }
}