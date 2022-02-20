// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Middleware.FildSecurityMiddlewareTestData
{
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Roles = "role1, role2")]
    public class RolesOnClass
    {
        [Authorize(Roles = "role2, role3")]
        public void RolesOnMethod()
        {
        }

        [Authorize]
        public void NoRolesOnMethod()
        {
        }
    }
}