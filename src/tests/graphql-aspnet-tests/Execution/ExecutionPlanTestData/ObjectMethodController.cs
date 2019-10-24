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

    [GraphRoute("objects")]
    public class ObjectMethodController : GraphController
    {
        [Query(typeof(ObjectMethodItem))]
        public Task<IGraphActionResult> RetrieveObject()
        {
            return this.Ok(new ObjectMethodItem()).AsCompletedTask();
        }
    }
}