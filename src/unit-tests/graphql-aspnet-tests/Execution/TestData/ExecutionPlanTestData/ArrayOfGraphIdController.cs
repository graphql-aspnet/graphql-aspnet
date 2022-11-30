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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Attributes;

    public class ArrayOfGraphIdController : GraphIdController
    {
        [QueryRoot]
        public bool IdsAccepted(IEnumerable<GraphId> ids)
        {
            return ids != null && ids.Count() == 3 &&
                ids.Any(x => x == "1") &&
                ids.Any(x => x == "2") &&
                ids.Any(x => x == "3");
        }
    }
}