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
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class UnionController : GraphController
    {
        [QueryRoot("retrieveUnion", "TwoOrTwo", typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))]
        public IGraphActionResult RetrieveUnion()
        {
            var result = new TwoPropertyObject()
            {
                Property1 = "prop1",
                Property2 = 5,
            };

            return this.Ok(result);
        }
    }
}