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
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Microsoft.AspNetCore.Authorization;

    public class PerFieldCounterController : GraphController
    {
        public static int NumberOfInvocations { get; set; }

        [QueryRoot("unsecured")]
        public TwoPropertyObject UnsecureMethod()
        {
            NumberOfInvocations += 1;
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
            NumberOfInvocations += 1;
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
            NumberOfInvocations += 1;
            return new TwoPropertyObject()
            {
                Property1 = "policy secured object",
                Property2 = 10,
            };
        }
    }
}