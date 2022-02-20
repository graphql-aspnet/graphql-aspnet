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

    [Authorize(AuthenticationSchemes = "scheme1, scheme2, scheme3")]
    public class SchemeOnClass
    {
        [Authorize(AuthenticationSchemes = "scheme1, scheme2, scheme3")]
        public void AllSchemesOnMethod()
        {
        }

        [Authorize(AuthenticationSchemes = "scheme1, scheme2")]
        public void SubsetOnMethod()
        {
        }

        [Authorize(AuthenticationSchemes = "scheme1")]
        public void SingleSchemeOnMethod()
        {
        }

        [Authorize]
        public void NoSchemesOnMethod()
        {
        }

        [Authorize(AuthenticationSchemes = "scheme4")]
        public void DifferentSchemeOnMethod()
        {
        }
    }
}