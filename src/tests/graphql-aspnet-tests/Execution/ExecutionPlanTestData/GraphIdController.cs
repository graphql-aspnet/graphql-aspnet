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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class GraphIdController : GraphController
    {
        [QueryRoot(typeof(GraphId))]
        public Task<IGraphActionResult> RetrieveId()
        {
            var id = new GraphId("abc123");
            return this.Ok(id).AsCompletedTask();
        }
    }
}