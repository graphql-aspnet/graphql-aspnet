// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.ControllerTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    [GraphRoute("other")]
    public class OtherController : GraphController
    {
        [Query]
        public Task<IGraphActionResult> AsyncActionMethod(string arg1 = "default")
        {
            return Task.FromResult(this.Ok("data result"));
        }
    }
}