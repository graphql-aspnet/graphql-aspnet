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

    [DirectiveLocations(ExecutableDirectiveLocation.FIELD)]
    [DirectiveLocations(ExecutableDirectiveLocation.FIELD | ExecutableDirectiveLocation.MUTATION)]
    [DirectiveLocations(TypeSystemDirectiveLocation.ENUM)]
    [DirectiveLocations(TypeSystemDirectiveLocation.OBJECT | TypeSystemDirectiveLocation.UNION)]
    [DirectiveLocations(TypeSystemDirectiveLocation.INPUT_OBJECT)]
    [DirectiveLocations(TypeSystemDirectiveLocation.INPUT_FIELD_DEFINITION)]
    public class OverlappingLocationsDirective : GraphDirective
    {
        public Task<IGraphActionResult> BeforeFieldResolution(int arg1, string arg2)
        {
            return null;
        }
    }
}