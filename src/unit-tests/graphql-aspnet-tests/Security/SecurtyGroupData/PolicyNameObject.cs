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
    using Microsoft.AspNetCore.Authorization;

    [Authorize("TestPolicy")]
    public class PolicyNameObject
    {
    }
}