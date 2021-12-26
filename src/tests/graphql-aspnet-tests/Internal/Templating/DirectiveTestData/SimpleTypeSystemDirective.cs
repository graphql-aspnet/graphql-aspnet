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
    using GraphQL.AspNet.Interfaces.TypeSystem;

    [DirectiveLocations(TypeSystemDirectiveLocation.ENUM)]
    public class SimpleTypeSystemDirective: GraphDirective
    {
        public IGraphActionResult AlterTypeSystem(ISchemaItem item)
        {
            return this.Ok();
        }
    }
}