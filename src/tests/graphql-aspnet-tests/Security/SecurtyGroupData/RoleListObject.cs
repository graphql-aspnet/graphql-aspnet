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

    [Authorize(Roles = "role1,role2,role3")]
    public class RoleListObject
    {
    }
}