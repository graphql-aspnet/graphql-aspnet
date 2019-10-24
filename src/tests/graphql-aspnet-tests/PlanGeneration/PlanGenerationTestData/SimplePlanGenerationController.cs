// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration.PlanGenerationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;

    [GraphRoute("simple")]
    public class SimplePlanGenerationController : GraphController
    {
        [Query]
        public TwoPropertyObject SimpleQueryMethod(string arg1 = "default string", long arg2 = 5)
        {
            return null;
        }

        [Query("unionQuery", "MyUnionType", typeof(TwoPropertyObject), typeof(TwoPropertyObjectV2))]
        public IGraphActionResult RetrieveUnionData()
        {
            return null;
        }

        [Query]
        public TwoPropertyObject ComplexQueryMethod(TwoPropertyObjectV2 arg1)
        {
            return null;
        }
    }
}