// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ExecutionDirectiveTestController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject RetrieveObject()
        {
            return new TwoPropertyObject()
            {
                Property1 = "Prop one value",
                Property2 = 5,
            };
        }
    }
}