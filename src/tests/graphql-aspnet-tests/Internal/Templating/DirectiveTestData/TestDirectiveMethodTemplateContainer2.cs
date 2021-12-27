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
    using GraphQL.AspNet.Schemas.TypeSystem;

    [DirectiveLocations(DirectiveLocation.FIELD | DirectiveLocation.FRAGMENT_SPREAD | DirectiveLocation.INLINE_FRAGMENT)]
    public class TestDirectiveMethodTemplateContainer2 : GraphDirective
    {
        [GraphSkip]
        public Task<IGraphActionResult> BeforeFieldResolution()
        {
            return Task.FromResult(null as IGraphActionResult);
        }

        public Task<IGraphActionResult> NotAValidMethodName()
        {
            return Task.FromResult(null as IGraphActionResult);
        }

        public Task AfterFieldResolution()
        {
            return null;
        }

        public IGraphActionResult AlterTypeSystem()
        {
            return null;
        }
    }
}