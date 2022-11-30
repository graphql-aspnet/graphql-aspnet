// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.GraphValidationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using Microsoft.AspNetCore.Authorization;

    public class ControllerWithSecurityPolicies : GraphController
    {
        [Authorize]
        [QueryRoot]
        public int SomeMethod()
        {
            return 0;
        }
    }
}