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

    [Authorize(AuthenticationSchemes = "testScheme1")]
    public class WithNestedMatchedSchemesOnAuthorize
    {
        // no way for one auth'd user to authenticate with both schemes (its one or the other).
        [Authorize(AuthenticationSchemes = "testScheme2, testScheme1")]
        public void TestMethod()
        {
        }
    }
}