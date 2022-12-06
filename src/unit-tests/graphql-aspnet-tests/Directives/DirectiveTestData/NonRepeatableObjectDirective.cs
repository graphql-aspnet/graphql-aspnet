// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives.DirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class NonRepeatableObjectDirective : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.OBJECT)]
        public IGraphActionResult Execute(string data)
        {
            return this.Ok();
        }
    }
}