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
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class ArrayScalarController : GraphController
    {
        [QueryRoot(typeof(IEnumerable<int>))]
        public IGraphActionResult RetrieveData()
        {
            return this.Ok(new int[] { 1, 3, 5 });
        }
    }
}