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
    using GraphQL.AspNet.Controllers;

    [GraphRoute("apples")]
    public class AppleController : FruitController
    {
        [Query]
        public int TotalApples()
        {
            return 85;
        }
    }
}