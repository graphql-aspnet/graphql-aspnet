// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using GraphQL.AspNet.Attributes;

    public class MixedReturnTypeController : GraphIdController
    {
        [QueryRoot]
        public MixedReturnTypeA CreateReturnObject()
        {
            return new MixedReturnTypeB()
            {
                Field1 = "FieldValue1",
                Field2 = "FieldValue2",
            };
        }
    }
}