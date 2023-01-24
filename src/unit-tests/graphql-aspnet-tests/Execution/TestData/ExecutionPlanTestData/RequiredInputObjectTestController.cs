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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class RequiredInputObjectTestController : GraphIdController
    {
        [QueryRoot]
        public string ReceiveObject(TwoPropertyObject obj)
        {
            return obj != null ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveObjectWithDefaultValue(TwoPropertyObject obj = null)
        {
            return obj != null ? "object supplied" : "object null";
        }

#nullable enable
        [QueryRoot]
        public string ReceiveNullableObject(TwoPropertyObject? obj)
        {
            return obj != null ? "object supplied" : "object null";
        }

        [QueryRoot]
        public string ReceiveNullableObjectWithDefaultValue(TwoPropertyObject? obj = null)
        {
            return obj != null ? "object supplied" : "object null";
        }
#nullable disable
    }
}