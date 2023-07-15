// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class ControllerWithInjectedService : GraphController
    {
        [QueryRoot]
        public int Add(int arg1, IInjectedService arg2)
        {
            return arg1 + arg2.FetchValue();
        }
    }
}