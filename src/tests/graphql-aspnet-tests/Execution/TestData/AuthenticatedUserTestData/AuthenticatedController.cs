// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.AuthenticatedUserTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using Microsoft.AspNetCore.Authorization;

    public class AuthenticatedController : GraphController
    {
        [QueryRoot]
        [Authorize(Policy = "User5")]
        public bool IsUserSupplied()
        {
            return this.User != null;
        }
    }
}