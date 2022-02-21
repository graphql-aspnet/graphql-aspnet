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

    [AllowAnonymous]
    public class AllowAnonymousOnClass
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