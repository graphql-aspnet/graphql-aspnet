// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Policy = "CustomPolicy")]
    [Authorize(Roles = "CustomRole1, CustomRole2")]
    public class DirectiveWithSecurityRequirements : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            return null;
        }
    }
}