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
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class InputArrayScalarController : GraphController
    {
        [QueryRoot]
        public int SumArray(int[] items)
        {
            return items.Sum();
        }
    }
}