// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ControllerTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Policy = "Policy1")]
    public class SecuredController : GraphController
    {
        [Query]
        [Authorize(Policy = "Policy2")]
        public string DoSomethingSecure()
        {
            return string.Empty;
        }

        [Query]
        public string DoSomething()
        {
            return string.Empty;
        }
    }
}