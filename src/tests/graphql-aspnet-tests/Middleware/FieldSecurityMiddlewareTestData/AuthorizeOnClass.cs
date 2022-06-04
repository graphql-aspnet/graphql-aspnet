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

    [Authorize]
    public class AuthorizeOnClass
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
    }
}