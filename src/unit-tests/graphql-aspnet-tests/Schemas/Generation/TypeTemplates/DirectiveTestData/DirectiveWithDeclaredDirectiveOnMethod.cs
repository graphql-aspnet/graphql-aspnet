// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class DirectiveWithDeclaredDirectiveOnMethod : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        [ApplyDirective(typeof(IncludeDirective))]
        public Task<IGraphActionResult> Execute(object source, int arg1, string arg2)
        {
            return Task.FromResult(null as IGraphActionResult);
        }
    }
}