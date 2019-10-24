// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Security.SecurtyGroupData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using Microsoft.AspNetCore.Authorization;

    public class SecuredController : GraphController
    {
        [QueryRoot("unsecured")]
        public TwoPropertyObject UnsecureMethod()
        {
            return new TwoPropertyObject()
            {
                Property1 = "unsecure object",
                Property2 = 1,
            };
        }

        [Authorize]
        [QueryRoot("secured")]
        public TwoPropertyObject SecureMethod()
        {
            return new TwoPropertyObject()
            {
                Property1 = "secured object",
                Property2 = 5,
            };
        }

        [Authorize]
        [QueryRoot("policySecured")]
        public TwoPropertyObject PolicySecuredMethod()
        {
            return new TwoPropertyObject()
            {
                Property1 = "policy secured object",
                Property2 = 10,
            };
        }
    }
}