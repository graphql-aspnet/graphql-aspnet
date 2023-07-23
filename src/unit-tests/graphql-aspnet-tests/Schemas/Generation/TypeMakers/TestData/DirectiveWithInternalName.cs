// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType(InternalName = "DirectiveInternalName_33")]
    public class DirectiveWithInternalName : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.AllTypeSystemLocations)]
        public IGraphActionResult Execute()
        {
            return null;
        }
    }
}