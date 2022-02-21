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

    public class NothingOnClass
    {
        [Authorize]
        public void AuthorizeOnMethod()
        {
        }

        [AllowAnonymous]
        public void AnonOnMethod()
        {
        }

        [Authorize]
        [AllowAnonymous]
        public void AnonAndAuthorizeOnMethod()
        {
        }

        public void NothingOnMethod()
        {
        }

        [Authorize("Policy1")]
        public void PolicyOnMethod()
        {
        }

        [Authorize(Roles = "role3, role4")]
        public void RolesOnMethod()
        {
        }
    }
}