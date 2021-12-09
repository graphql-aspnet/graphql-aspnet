// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Middleware.FieldSecurityPipelineTestData
{
    using Microsoft.AspNetCore.Authorization;

    [AllowAnonymous]
    public class AllowAnonymousOnAuthorize
    {
    }
}