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

    [DirectiveLocations(DirectiveLocation.SCALAR)]
    public class TypeSystemDirectiveWithMethod : GraphDirective
    {
        // no "AlterTypeSystem" method defined
        public Task<IGraphActionResult> AlterTypeSystem(ISchemaItem item)
        {
            return null;
        }
    }
}