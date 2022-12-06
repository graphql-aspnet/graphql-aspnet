// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.FieldAuthorizerTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using Microsoft.AspNetCore.Authorization;

    [Authorize("RequiresPolicy5")]
    public class Controller_Policy_RequiresPolicy5 : GraphController
    {
        [Query(typeof(string))]
        [Authorize("RequiresRole1")]
        public Task<IGraphActionResult> Policy_RequiresRole1()
        {
            return this.Ok(string.Empty).AsCompletedTask();
        }
    }
}