// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.RuntimeDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class SingleFieldController : GraphController
    {
        [QueryRoot]
        public int Field()
        {
            return 99;
        }
    }
}