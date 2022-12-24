// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Engine.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class DirectiveWithArgs : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.AllTypeSystemLocations)]
        public IGraphActionResult Execute(int arg1, string arg2)
        {
            return this.Ok();
        }
    }
}