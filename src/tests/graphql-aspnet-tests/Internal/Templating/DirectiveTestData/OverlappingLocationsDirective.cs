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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [DirectiveLocations(DirectiveLocation.FIELD)]
    [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.MUTATION)]
    [DirectiveLocations(DirectiveLocation.ENUM)]
    [DirectiveLocations(DirectiveLocation.OBJECT | DirectiveLocation.UNION)]
    [DirectiveLocations(DirectiveLocation.INPUT_OBJECT)]
    [DirectiveLocations(DirectiveLocation.INPUT_FIELD_DEFINITION)]
    public class OverlappingLocationsDirective : GraphDirective
    {
        public Task<IGraphActionResult> BeforeFieldResolution(int arg1, string arg2)
        {
            return null;
        }

        public IGraphActionResult AlterTypeSystem(ISchemaItem item)
        {
            return null;
        }
    }
}