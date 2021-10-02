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
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class ArrayScalarController : GraphIdController
    {
        [QueryRoot(typeof(IEnumerable<int>))]
        public IGraphActionResult RetrieveData()
        {
            return this.Ok(new int[] { 1, 3, 5 });
        }
    }
}